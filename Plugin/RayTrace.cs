using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Memory;
using RayTraceAPI;
using System.Runtime.InteropServices;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace FortniteEmotes;

public static class RayTraceBridge
{
    public static nint Handle;
    public static bool Loaded;

    public static unsafe Func<nint, nint, nint, nint, nint, nint, bool>? TraceShape;
    public static unsafe Func<nint, nint, nint, nint, nint, nint, bool>? TraceEndShape;
    public static unsafe Func<nint, nint, nint, nint, nint, nint, nint, nint, bool>? TraceHullShape;
    public static unsafe Func<nint, nint, nint, nint, nint, nint, bool>? TraceShapeEx;

    public static bool Initialize()
    {
        Handle = (nint)Utilities.MetaFactory("CRayTraceInterface002")!;

        if (Handle == 0)
            return false;

        Bind();
        Loaded = true;
        return true;
    }

    private static void Bind()
    {
        int shape, end, hull, ex;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            shape = 1;
            end = 2;
            hull = 3;
            ex = 4;
        }
        else
        {
            shape = 2;
            end = 3;
            hull = 4;
            ex = 5;
        }

        TraceShape = VirtualFunction.Create<nint, nint, nint, nint, nint, nint, bool>(Handle, shape);
        TraceEndShape = VirtualFunction.Create<nint, nint, nint, nint, nint, nint, bool>(Handle, end);
        TraceHullShape = VirtualFunction.Create<nint, nint, nint, nint, nint, nint, nint, nint, bool>(Handle, hull);
        TraceShapeEx = VirtualFunction.Create<nint, nint, nint, nint, nint, nint, bool>(Handle, ex);
    }
}

public class CRayTrace : CRayTraceInterface
{
    public bool TraceShape(Vector start, QAngle angles, CEntityInstance? ignore, TraceOptions options, out TraceResult result)
    {
        unsafe
        {
            result = default;

            if (!RayTraceBridge.Loaded)
                return false;

            TraceResult resultBuffer = default;
            TraceOptions optionsBuffer = options;

            bool success = RayTraceBridge.TraceShape!(RayTraceBridge.Handle,
                start.Handle,
                angles.Handle,
                ignore?.Handle ?? nint.Zero,
                (nint)(&optionsBuffer),
                (nint)(&resultBuffer));

            result = resultBuffer;
            return success;
        }
    }

    public bool TraceEndShape(Vector start, Vector end, CEntityInstance? ignore, TraceOptions options, out TraceResult result)
    {
        unsafe
        {
            result = default;

            if (!RayTraceBridge.Loaded)
                return false;

            TraceResult resultBuffer = default;
            TraceOptions optionsBuffer = options;

            bool success = RayTraceBridge.TraceEndShape!(RayTraceBridge.Handle,
                start.Handle,
                end.Handle,
                ignore?.Handle ?? nint.Zero,
                (nint)(&optionsBuffer),
                (nint)(&resultBuffer));

            result = resultBuffer;
            return success;
        }
    }

    public bool TraceHullShape(Vector start, Vector end, Vector mins, Vector maxs, CEntityInstance? ignore, TraceOptions options, out TraceResult result)
    {
        unsafe
        {
            result = default;

            if (!RayTraceBridge.Loaded)
                return false;

            TraceResult resultBuffer = default;
            TraceOptions optionsBuffer = options;

            bool success = RayTraceBridge.TraceHullShape!(RayTraceBridge.Handle,
                start.Handle,
                end.Handle,
                mins.Handle,
                maxs.Handle,
                ignore?.Handle ?? nint.Zero,
                (nint)(&optionsBuffer),
                (nint)(&resultBuffer));

            result = resultBuffer;
            return success;
        }
    }

    public bool TraceShapeEx(Vector start, Vector end, nint filter, nint ray, out TraceResult result)
    {
        unsafe
        {
            result = default;

            if (!RayTraceBridge.Loaded)
                return false;

            TraceResult resultBuffer = default;

            bool success = RayTraceBridge.TraceShapeEx!(RayTraceBridge.Handle,
                start.Handle,
                end.Handle,
                filter,
                ray,
                (nint)(&resultBuffer));

            result = resultBuffer;
            return success;
        }
    }
}