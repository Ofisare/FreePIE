extern "C"
{
  #include "../include/ovr_freepie.h"
}

#include <OVR_CAPI.h>
#include <extras/OVR_Math.h>

ovrSession HMD;


double HmdFrameTiming;

int ovr_freepie_init()
{
	ovrResult result = ovr_Initialize(nullptr);
	if (!OVR_SUCCESS(result))
		return 1;

	ovrGraphicsLuid luid;
	result = ovr_Create(&HMD, &luid);

	if (!OVR_SUCCESS(result))
		return 1;

	if (!HMD)
		return 1;

	return 0;
}

int ovr_freepie_reset_orientation()
{
	ovr_RecenterTrackingOrigin(HMD);
	return 0;
}

int ovr_freepie_setPose(ovrPosef *pose, ovr_freepie_6dof *dof)
{
	OVR::Matrix4<float> matrix = OVR::Matrix4<float>(pose->Orientation);
	
	dof->left[0] = matrix.M[0][0];
	dof->left[1] = matrix.M[1][0];
	dof->left[2] = matrix.M[2][0];

	dof->up[0] = matrix.M[0][1];
	dof->up[1] = matrix.M[1][1];
	dof->up[2] = matrix.M[2][1];

	dof->forward[0] = matrix.M[0][2];
	dof->forward[1] = matrix.M[1][2];
	dof->forward[2] = matrix.M[2][2];

	dof->position[0] = pose->Position.x;
	dof->position[1] = pose->Position.y;
	dof->position[2] = pose->Position.z;
	return  0;
}

int ovr_freepie_read(ovr_freepie_data *output)
{
	HmdFrameTiming = ovr_GetPredictedDisplayTime(HMD, 0);
	ovrTrackingState ts = ovr_GetTrackingState(HMD, ovr_GetTimeInSeconds(), HmdFrameTiming);
	ovrSessionStatus sessionStatus;

	ovrResult result = ovr_GetSessionStatus(HMD, &sessionStatus);

	output->HmdMounted = sessionStatus.HmdMounted;
	output->statusHead = ts.StatusFlags == (ovrStatus_OrientationTracked | ovrStatus_PositionTracked) ? 2 : ts.StatusFlags > 0 ? 1 : 0;
	output->statusLeftHand = ts.HandStatusFlags[ovrHand_Left] == (ovrStatus_OrientationTracked | ovrStatus_PositionTracked) ? 2 : ts.HandStatusFlags[ovrHand_Left] > 0 ? 1 : 0;
	output->statusRightHand = ts.HandStatusFlags[ovrHand_Right] == (ovrStatus_OrientationTracked | ovrStatus_PositionTracked) ? 2 : ts.HandStatusFlags[ovrHand_Right] > 0 ? 1 : 0;

	ovrPosef headpose = ts.HeadPose.ThePose;
	ovrPosef lhandPose = ts.HandPoses[ovrHand_Left].ThePose;
	ovrPosef rhandPose = ts.HandPoses[ovrHand_Right].ThePose;
	ovr_freepie_setPose(&headpose, &output->head);
	ovr_freepie_setPose(&lhandPose, &output->leftHand);
	ovr_freepie_setPose(&rhandPose, &output->rightHand);

	ovrInputState inputState;
	if (OVR_SUCCESS(ovr_GetInputState(HMD, ovrControllerType_Touch, &inputState)))
	{
		output->A = ((inputState.Buttons & ovrButton_A) == ovrButton_A) ? 1 : ((inputState.Touches & ovrTouch_A) == ovrTouch_A) ? 0.5 : 0;
		output->B = ((inputState.Buttons & ovrButton_B) == ovrButton_B) ? 1 : ((inputState.Touches & ovrTouch_B) == ovrTouch_B) ? 0.5 : 0;
		output->X = ((inputState.Buttons & ovrButton_X) == ovrButton_X) ? 1 : ((inputState.Touches & ovrTouch_X) == ovrTouch_X) ? 0.5 : 0;
		output->Y = ((inputState.Buttons & ovrButton_Y) == ovrButton_Y) ? 1 : ((inputState.Touches & ovrTouch_Y) == ovrTouch_Y) ? 0.5 : 0;
		output->LThumb = ((inputState.Buttons & ovrButton_LThumb) == ovrButton_LThumb) ? 1 : ((inputState.Touches & ovrTouch_LThumb) == ovrTouch_LThumb) ? 0.5 : 0;
		output->RThumb = ((inputState.Buttons & ovrButton_RThumb) == ovrButton_RThumb) ? 1 : ((inputState.Touches & ovrTouch_RThumb) == ovrTouch_RThumb) ? 0.5 : 0;
		output->Menu = ((inputState.Buttons & ovrButton_Enter) == ovrButton_Enter) ? 1 : 0;
		output->Home = ((inputState.Buttons & ovrButton_Home) == ovrButton_Home) ? 1 : 0;

		output->LTrigger = inputState.IndexTrigger[ovrHand_Left];
		output->LGrip = inputState.HandTrigger[ovrHand_Left];
		output->Lstick[0] = inputState.Thumbstick[ovrHand_Left].x;
		output->Lstick[1] = inputState.Thumbstick[ovrHand_Left].y;
		output->RTrigger = inputState.IndexTrigger[ovrHand_Right];
		output->RGrip = inputState.HandTrigger[ovrHand_Right];
		output->Rstick[0] = inputState.Thumbstick[ovrHand_Right].x;
		output->Rstick[1] = inputState.Thumbstick[ovrHand_Right].y;
	}

	return 0;
}
int ovr_freepie_trigger_haptic_pulse(unsigned int controllerIndex, unsigned int durationMicroSec, float frequency, float amplitude)
{
	if (controllerIndex == 0)
	{
		ovr_SetControllerVibration(HMD, ovrControllerType_LTouch, frequency, amplitude);
	}
	else
	{
		ovr_SetControllerVibration(HMD, ovrControllerType_RTouch, frequency, amplitude);
	}

	return 0;
}

int ovr_freepie_destroy()
{
	ovr_Destroy(HMD);
	ovr_Shutdown();

	return 0;
}