using System;

namespace XFlow.Modules.Box2D.ClientServer.Api
{
    [Flags]
    public enum Box2DDebugDrawFlags
    {
        ShapeBit			= 1,	// draw shapes
        JointBit			= 2,	// draw joint connections
        AabbBit				= 4,	// draw axis aligned bounding boxes
        PairBit				= 8,	// draw broad-phase pairs
        CenterOfMassBit		= 16,	// draw center of mass frame
        ContactBit			= 32	// draw contact info
    };
    
}