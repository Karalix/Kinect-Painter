using System;
using Microsoft.Kinect.Toolkit.Interaction;

public class DummyInteraction : IInteractionClient
{
	public DummyInteraction()
	{
	}

    public InteractionInfo GetInteractionInfoAtLocation(int skeletonTrackingID, InteractionHandType handType, double x, double y)
    {
        InteractionInfo res = new InteractionInfo();
        res.IsGripTarget = true;
        res.IsPressTarget = true;
        res.PressAttractionPointX = 0.5;
        res.PressAttractionPointY = 0.5;
        res.PressTargetControlId = 1;

        return res;
    }

}
