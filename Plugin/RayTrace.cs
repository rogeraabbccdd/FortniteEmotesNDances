using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Memory;
using RayTraceAPI;
using System.Runtime.InteropServices;
using System.Numerics;

namespace FortniteEmotes;

public static class RayTraceBridge
{
    public static nint m_pRayTraceHandle = nint.Zero;
    public static bool m_bRayTraceLoaded = false;

    public static Func<nint, nint, nint, nint, nint, nint, bool>? _traceShape;
    public static Func<nint, nint, nint, nint, nint, nint, bool>? _traceEndShape;
    public static Func<nint, nint, nint, nint, nint, nint, nint, nint, bool>? _traceHullShape;

    public static bool Initialize()
    {
        m_pRayTraceHandle = (nint)Utilities.MetaFactory("CRayTraceInterface001")!;

        if (m_pRayTraceHandle == nint.Zero)
        {
            return false;
        }

        Bind();
        m_bRayTraceLoaded = true;
        return true;
    }

    private static void Bind()
    {
        int traceShapeIndex = 2;
        int traceEndShapeIndex = 3;
        int traceHullShapeIndex = 4;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            traceShapeIndex = 1;
            traceEndShapeIndex = 2;
            traceHullShapeIndex = 3;
        }

        _traceShape = VirtualFunction.Create<nint, IntPtr, IntPtr, nint, nint, nint, bool>(m_pRayTraceHandle, traceShapeIndex);
        _traceEndShape = VirtualFunction.Create<nint, IntPtr, IntPtr, nint, nint, nint, bool>(m_pRayTraceHandle, traceEndShapeIndex);
        _traceHullShape = VirtualFunction.Create<nint, IntPtr, IntPtr, IntPtr, IntPtr, nint, nint, nint, bool>(m_pRayTraceHandle, traceHullShapeIndex);
    }
}

public class CRayTrace : CRayTraceInterface
{
    public unsafe bool TraceShape(Vector3 origin, Vector3 angles, CBaseEntity? ignoreEntity, TraceOptions options, out TraceResult result)
    {
        result = default;

        if (!RayTraceBridge.m_bRayTraceLoaded || RayTraceBridge.m_pRayTraceHandle == nint.Zero)
            return false;

        TraceResult resultBuffer = default;
        TraceOptions optionsBuffer = options;

        var originPtr = &origin;
        var anglesPtr = &angles;

        bool success = RayTraceBridge._traceShape!(RayTraceBridge.m_pRayTraceHandle,
            (nint)originPtr,
            (nint)anglesPtr,
            ignoreEntity?.Handle ?? nint.Zero,
            (nint)(&optionsBuffer),
            (nint)(&resultBuffer));

        result = resultBuffer;
        return success;
    }

    public unsafe bool TraceEndShape(Vector3 origin, Vector3 endOrigin, CBaseEntity? ignoreEntity, TraceOptions options, out TraceResult result)
    {
        result = default;

        if (!RayTraceBridge.m_bRayTraceLoaded || RayTraceBridge.m_pRayTraceHandle == nint.Zero)
            return false;

        TraceResult resultBuffer = default;
        TraceOptions optionsBuffer = options;

        var originPtr = &origin;
        var endOriginPtr = &endOrigin;

        bool success = RayTraceBridge._traceEndShape!(RayTraceBridge.m_pRayTraceHandle,
            (nint)originPtr,
            (nint)endOriginPtr,
            ignoreEntity?.Handle ?? nint.Zero,
            (nint)(&optionsBuffer),
            (nint)(&resultBuffer));

        result = resultBuffer;
        return success;
    }

    public unsafe bool TraceHullShape(Vector3 vecStart, Vector3 vecEnd, Vector3 hullMins, Vector3 hullMaxs, CBaseEntity? ignoreEntity, TraceOptions options, out TraceResult result)
    {
        result = default;

        if (!RayTraceBridge.m_bRayTraceLoaded || RayTraceBridge.m_pRayTraceHandle == nint.Zero)
            return false;

        TraceResult resultBuffer = default;
        TraceOptions optionsBuffer = options;

        var vecStartPtr = &vecStart;
        var vecEndPtr = &vecEnd;
        var hullMinsPtr = &hullMins;
        var hullMaxsPtr = &hullMaxs;

        bool success = RayTraceBridge._traceHullShape!(RayTraceBridge.m_pRayTraceHandle,
            (nint)vecStartPtr,
            (nint)vecEndPtr,
            (nint)hullMinsPtr,
            (nint)hullMaxsPtr,
            ignoreEntity?.Handle ?? nint.Zero,
            (nint)(&optionsBuffer),
            (nint)(&resultBuffer));

        result = resultBuffer;
        return success;
    }
}