using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Devices.DeviceAndDriverInstallation;

namespace DAWTomator;

public record class DeviceInfo(string? HwId, string? Description, string? FriendlyName, bool? Enabled)
{
    public override string ToString() => (Description, FriendlyName) is not (null, null) ?
        $"{FriendlyName} [{Description}]" : $"? {HwId}";
}

public static class HardwareManager
{
    const int ERROR_NO_MORE_ITEMS = 259;
    const int ERROR_INVALID_DATA = 13;
    const int ERROR_INSUFFICIENT_BUFFER = 122;
    const int ERROR_ACCESS_DENIED = 5;

    public static IEnumerable<DeviceInfo> FilterKeywords(this IEnumerable<DeviceInfo> devices, string[] keywords) =>
        devices.Where(d => d.ToString() is string s && keywords.Any(k => s.Contains(k, StringComparison.OrdinalIgnoreCase)));

    public static unsafe IEnumerable<DeviceInfo> GetDevices()
    {
        using SetupDiDestroyDeviceInfoListSafeHandle infoSet =
            PInvoke.SetupDiGetClassDevs(Guid.Empty, null, default, SETUP_DI_GET_CLASS_DEVS_FLAGS.DIGCF_ALLCLASSES);
        if (infoSet.IsInvalid) CheckError("SetupDiGetClassDevs");

        SP_DEVINFO_DATA deviceData = default;
        for (int i = 0; ; i++)
        {
            deviceData.cbSize = (uint)Unsafe.SizeOf<SP_DEVINFO_DATA>();
            if (!PInvoke.SetupDiEnumDeviceInfo(infoSet, (uint)i, ref deviceData))
            {
                int error = Marshal.GetLastWin32Error();
                if (error == ERROR_NO_MORE_ITEMS)
                {
                    break;
                }
                CheckError("SetupDiEnumDeviceInfo", error);
            }

            bool? enabled = null;
            CONFIGRET ret = PInvoke.CM_Get_DevNode_Status(out CM_DEVNODE_STATUS_FLAGS status, out CM_PROB problem, deviceData.DevInst, 0);
            
            if (ret == CONFIGRET.CR_SUCCESS)
            {
                if ((status & CM_DEVNODE_STATUS_FLAGS.DN_HAS_PROBLEM) == 0)
                {
                    enabled = true;
                }
                else if (problem == CM_PROB.CM_PROB_DISABLED)
                {
                    enabled = false;
                }
            }

            string? hwId = GetStringPropertyForDevice(infoSet, deviceData, SETUP_DI_REGISTRY_PROPERTY.SPDRP_HARDWAREID);
            string? friendlyName = GetStringPropertyForDevice(infoSet, deviceData, SETUP_DI_REGISTRY_PROPERTY.SPDRP_FRIENDLYNAME);
            string? description = GetStringPropertyForDevice(infoSet, deviceData, SETUP_DI_REGISTRY_PROPERTY.SPDRP_DEVICEDESC);
            yield return new DeviceInfo(hwId, description, friendlyName, enabled);
        }
    }

    internal static void SetEnabled(DeviceInfo info, bool enabled)
    {
        ArgumentException.ThrowIfNullOrEmpty(info.HwId, nameof(info));

        using SetupDiDestroyDeviceInfoListSafeHandle infoSet =
            PInvoke.SetupDiGetClassDevs(Guid.Empty, null, default, SETUP_DI_GET_CLASS_DEVS_FLAGS.DIGCF_ALLCLASSES);
        if (infoSet.IsInvalid) CheckError("SetupDiGetClassDevs");

        SP_DEVINFO_DATA deviceData = default;
        for (int i = 0; ; i++)
        {
            deviceData.cbSize = (uint)Unsafe.SizeOf<SP_DEVINFO_DATA>();
            if (!PInvoke.SetupDiEnumDeviceInfo(infoSet, (uint)i, ref deviceData))
            {
                int error = Marshal.GetLastWin32Error();
                if (error == ERROR_NO_MORE_ITEMS)
                {
                    throw new Exception($"Device not found! {info}");
                }
                CheckError("SetupDiEnumDeviceInfo", error);
            }

            string? hwId = GetStringPropertyForDevice(infoSet, deviceData, SETUP_DI_REGISTRY_PROPERTY.SPDRP_HARDWAREID);
            
            if (hwId == info.HwId)
            {
                break;
            }
        }

        SP_CLASSINSTALL_HEADER header = default;
        header.cbSize = (uint)Unsafe.SizeOf<SP_CLASSINSTALL_HEADER>();
        header.InstallFunction = DI_FUNCTION.DIF_PROPERTYCHANGE;

        SP_PROPCHANGE_PARAMS propChangeParams = new SP_PROPCHANGE_PARAMS
        {
            ClassInstallHeader = header,
            StateChange = enabled ? SETUP_DI_STATE_CHANGE.DICS_ENABLE : SETUP_DI_STATE_CHANGE.DICS_DISABLE,
            Scope = SETUP_DI_PROPERTY_CHANGE_SCOPE.DICS_FLAG_GLOBAL,
            HwProfile = 0
        };

        if (!SetupDiSetClassInstallParams(infoSet, deviceData, propChangeParams))
        {
            CheckError("SetupDiSetClassInstallParams");
        }

        if (!PInvoke.SetupDiChangeState(infoSet, ref deviceData))
        {
            int error = Marshal.GetLastWin32Error();
            if (error == ERROR_ACCESS_DENIED)
            {
                throw new UnauthorizedAccessException("Acccess denied!");
            }
            CheckError("SetupDiChangeState", error);
        }
    }

    private static unsafe Windows.Win32.Foundation.BOOL SetupDiSetClassInstallParams(SafeHandle DeviceInfoSet, in SP_DEVINFO_DATA DeviceInfoData, in SP_PROPCHANGE_PARAMS ClassInstallParams)
    {
        bool DeviceInfoSetAddRef = false;
        try
        {
           HDEVINFO DeviceInfoSetLocal;
            if (DeviceInfoSet is object)
            {
                DeviceInfoSet.DangerousAddRef(ref DeviceInfoSetAddRef);
                DeviceInfoSetLocal = (HDEVINFO)DeviceInfoSet.DangerousGetHandle();
            }
            else
                throw new ArgumentNullException(nameof(DeviceInfoSet));

            fixed (SP_DEVINFO_DATA* pDeviceInfoData = &DeviceInfoData)
            fixed (SP_PROPCHANGE_PARAMS* pClassInstallParams = &ClassInstallParams)
            {
                Windows.Win32.Foundation.BOOL __result = PInvoke.SetupDiSetClassInstallParams(DeviceInfoSetLocal, pDeviceInfoData, (SP_CLASSINSTALL_HEADER*)pClassInstallParams, (uint)Unsafe.SizeOf<SP_PROPCHANGE_PARAMS>());
                return __result;
            }
        }
        finally
        {
            if (DeviceInfoSetAddRef)
                DeviceInfoSet.DangerousRelease();
        }
    }

    private static void CheckError(string operataion, int lasterror = -1)
    {
        int code = lasterror == -1 ? Marshal.GetLastWin32Error() : lasterror;
        if (code != 0) throw new Exception($"{operataion} failed with code {code}.");
    }

    private static unsafe string? GetStringPropertyForDevice(
        SetupDiDestroyDeviceInfoListSafeHandle info,
        SP_DEVINFO_DATA devdata,
        SETUP_DI_REGISTRY_PROPERTY propId)
    {
        uint propType = 0;
        int outSize = 0;
        int rentSize = 32;

RentPlz:
        byte[] buffer = ArrayPool<byte>.Shared.Rent(rentSize);
        try
        { 
            if (!PInvoke.SetupDiGetDeviceRegistryProperty(info, devdata, propId, &propType, buffer, (uint*)&outSize))
            {
                int error = Marshal.GetLastWin32Error();
                if (error == ERROR_INSUFFICIENT_BUFFER)
                {
                    rentSize = outSize;
                    goto RentPlz;
                }
                else if (error == ERROR_INVALID_DATA)
                {
                    return null;
                }
                else CheckError("SetupDiGetDeviceRegistryPropertyW", error);
            }

            ReadOnlySpan<byte> bytes = buffer.AsSpan(0, outSize);
            ReadOnlySpan<byte> zeros = stackalloc byte[2];
            if (bytes.EndsWith(zeros)) bytes = bytes[..^2];
            return Encoding.Unicode.GetString(bytes);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }   
}