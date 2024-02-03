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
	ovrInitParams initParams = { ovrInit_RequestVersion + ovrInit_Invisible, OVR_MINOR_VERSION, NULL, 0, 0 };
	ovrResult result = ovr_Initialize(&initParams);
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

	if (OVR_SUCCESS(ovr_GetSessionStatus(HMD, &sessionStatus)))
	{
		output->HmdMounted = sessionStatus.HmdMounted;
		output->StatusHead = ts.StatusFlags == (ovrStatus_OrientationTracked | ovrStatus_PositionTracked) ? 2 : ts.StatusFlags > 0 ? 1 : 0;
		output->StatusLeftHand = ts.HandStatusFlags[ovrHand_Left] == (ovrStatus_OrientationTracked | ovrStatus_PositionTracked) ? 2 : ts.HandStatusFlags[ovrHand_Left] > 0 ? 1 : 0;
		output->StatusRightHand = ts.HandStatusFlags[ovrHand_Right] == (ovrStatus_OrientationTracked | ovrStatus_PositionTracked) ? 2 : ts.HandStatusFlags[ovrHand_Right] > 0 ? 1 : 0;

		ovrPosef headpose = ts.HeadPose.ThePose;
		ovrPosef lhandPose = ts.HandPoses[ovrHand_Left].ThePose;
		ovrPosef rhandPose = ts.HandPoses[ovrHand_Right].ThePose;
		ovr_freepie_setPose(&headpose, &output->head);
		ovr_freepie_setPose(&lhandPose, &output->leftHand);
		ovr_freepie_setPose(&rhandPose, &output->rightHand);
	}

	ovrInputState inputState;
	if (OVR_SUCCESS(ovr_GetInputState(HMD, ovrControllerType_Touch, &inputState)))
	{
		output->LeftTrigger = inputState.IndexTrigger[ovrHand_Left];
		output->LeftGrip = inputState.HandTrigger[ovrHand_Left];
		output->LeftStickAxes[0] = inputState.Thumbstick[ovrHand_Left].x;
		output->LeftStickAxes[1] = inputState.Thumbstick[ovrHand_Left].y;
		output->RightTrigger = inputState.IndexTrigger[ovrHand_Right];
		output->RightGrip = inputState.HandTrigger[ovrHand_Right];
		output->RightStickAxes[0] = inputState.Thumbstick[ovrHand_Right].x;
		output->RightStickAxes[1] = inputState.Thumbstick[ovrHand_Right].y;

		output->LeftButtonsPressed = output->RightButtonsPressed = inputState.Buttons;
		output->LeftButtonsTouched = output->RightButtonsTouched = inputState.Touches;
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

	//unsigned char amplitudeChar = (uint8_t)255;// round(amplitude * 255);
	//ovrControllerType_ controller = ovrControllerType_LTouch;
	//if (controllerIndex > 0)
	//	controller = ovrControllerType_RTouch;
	//
	//ovrTouchHapticsDesc desc = ovr_GetTouchHapticsDesc(HMD, controller);
	//
	//ovrHapticsPlaybackState state;
	//ovrResult result = ovr_GetControllerVibrationState(HMD, controller, &state);
	//if (result != ovrSuccess || state.SamplesQueued >= desc.QueueMinSizeToAvoidStarvation)
	//{
	//	return 1;
	//}
	//
	//unsigned char* samples = new unsigned char [desc.SubmitOptimalSamples];
	//for (int32_t i = 0; i < desc.SubmitOptimalSamples; ++i)
	//	samples[i] = amplitudeChar;
	//
	//ovrHapticsBuffer buffer;
	//buffer.SubmitMode = ovrHapticsBufferSubmit_Enqueue;
	//buffer.SamplesCount = desc.SubmitOptimalSamples;
	//buffer.Samples = samples;
	//result = ovr_SubmitControllerVibration(HMD, controller, &buffer);
	//delete[] samples;
	//
	//if (result != ovrSuccess)
	//{
	//	return 1;
	//}
	//
	//result = ovr_GetControllerVibrationState(HMD, controller, &state);
	//if (result != ovrSuccess || state.SamplesQueued >= desc.QueueMinSizeToAvoidStarvation)
	//{
	//	return 1;
	//}
	
	return 0;
}

int ovr_freepie_destroy()
{
	ovr_Destroy(HMD);
	ovr_Shutdown();

	return 0;
}