extern "C"
{
  #include "../include/ovr_freepie.h"
}
#include <openvr.h>

vr::IVRSystem* m_system;
unsigned int leftID;
unsigned int rightID;
unsigned int triggerID;
unsigned int gripID;
unsigned int stickID;

double	HmdFrameTiming;

int ovr_freepie_init()
{
	if (vr::VR_IsHmdPresent() == false)
		return 1;

	vr::EVRInitError eError = vr::VRInitError_None;
	m_system = vr::VR_Init(&eError, vr::VRApplication_Other);
	if (eError != vr::VRInitError_None)
		return 1;		

	return 0;
}



int ovr_freepie_reset_orientation()
{
	//ovr_RecenterTrackingOrigin(HMD);
	return 0;
}

int ovr_freepie_setPose(vr::TrackedDevicePose_t* pose, ovr_freepie_6dof *dof)
{
	vr::HmdMatrix34_t matrix = pose->mDeviceToAbsoluteTracking;

	dof->left[0] = matrix.m[0][0];
	dof->left[1] = matrix.m[1][0];
	dof->left[2] = matrix.m[2][0];

	dof->up[0] = matrix.m[0][1];
	dof->up[1] = matrix.m[1][1];
	dof->up[2] = matrix.m[2][1];

	dof->forward[0] = matrix.m[0][2];
	dof->forward[1] = matrix.m[1][2];
	dof->forward[2] = matrix.m[2][2];

	dof->position[0] = matrix.m[0][3];
	dof->position[1] = matrix.m[1][3];
	dof->position[2] = matrix.m[2][3];
	return  0;
}

int ovr_freepie_read(ovr_freepie_data *output)
{
	if (!leftID)
	{
		for (unsigned int id = 0; id < vr::k_unMaxTrackedDeviceCount; id++) {
			vr::ETrackedDeviceClass trackedDeviceClass = m_system->GetTrackedDeviceClass(id);
			if (trackedDeviceClass != vr::ETrackedDeviceClass::TrackedDeviceClass_Controller || !m_system->IsTrackedDeviceConnected(id))
				continue;

			// Confirmed that the device in question is a connected controller
			vr::ETrackedControllerRole role = m_system->GetControllerRoleForTrackedDeviceIndex(id);
			if (role == vr::TrackedControllerRole_LeftHand)
			{
				leftID = id;

				for (int x = 0; x < vr::k_unControllerStateAxisCount; x++)
				{
					int prop = m_system->GetInt32TrackedDeviceProperty(id, (vr::ETrackedDeviceProperty)(vr::Prop_Axis0Type_Int32 + x));

					if (prop == vr::k_eControllerAxis_Trigger)
						if (triggerID == 0)
							triggerID = x;
						else
							gripID = x;
					else if (prop == vr::k_eControllerAxis_TrackPad || prop == vr::k_eControllerAxis_Joystick)
						stickID = x;
				}
			}
			else if (role == vr::TrackedControllerRole_RightHand)
			{
				rightID = id;
			}
		}
	}

	vr::TrackedDevicePose_t headTrackedDevicePose;

	m_system->GetDeviceToAbsoluteTrackingPose(vr::TrackingUniverseStanding, 0, &headTrackedDevicePose, 1);
	ovr_freepie_setPose(&headTrackedDevicePose, &output->head);
	output->StatusHead = headTrackedDevicePose.eTrackingResult == vr::TrackingResult_Running_OK ? 2 : headTrackedDevicePose.eTrackingResult > vr::TrackingResult_Running_OK ? 1 : 0;

	vr::TrackedDevicePose_t leftTrackedDevicePose;
	vr::VRControllerState_t lefControllerState;
	m_system->GetControllerStateWithPose(vr::TrackingUniverseStanding, leftID, &lefControllerState, sizeof(lefControllerState), &leftTrackedDevicePose);
	ovr_freepie_setPose(&leftTrackedDevicePose, &output->leftHand);
	output->StatusLeftHand = leftTrackedDevicePose.eTrackingResult == vr::TrackingResult_Running_OK ? 2 : leftTrackedDevicePose.eTrackingResult > vr::TrackingResult_Running_OK ? 1 : 0;
	output->LeftTrigger = lefControllerState.rAxis[triggerID].x;
	output->LeftGrip = lefControllerState.rAxis[gripID].x;
	output->LeftStickAxes[0] = lefControllerState.rAxis[stickID].x;
	output->LeftStickAxes[1] = lefControllerState.rAxis[stickID].y;

	uint64_t buttonA = vr::ButtonMaskFromId(vr::k_EButton_A);
	uint64_t buttonB = vr::ButtonMaskFromId(vr::k_EButton_IndexController_B);
	uint64_t buttonStick = vr::ButtonMaskFromId(vr::k_EButton_Axis0);
	uint64_t buttonMenu = vr::ButtonMaskFromId(vr::k_EButton_System);
	uint64_t buttonThumb = vr::ButtonMaskFromId(vr::k_EButton_ProximitySensor);

	output->X = ((lefControllerState.ulButtonPressed & buttonA) == buttonA) ? 1 : ((lefControllerState.ulButtonPressed & buttonA) == buttonA) ? 0.5 : 0;
	output->Y = ((lefControllerState.ulButtonPressed & buttonB) == buttonB) ? 1 : ((lefControllerState.ulButtonPressed & buttonB) == buttonB) ? 0.5 : 0;
	output->LeftStick = ((lefControllerState.ulButtonPressed & buttonStick) == buttonStick) ? 1 : ((lefControllerState.ulButtonPressed & buttonStick) == buttonStick) ? 0.5 : 0;
	output->Menu = ((lefControllerState.ulButtonPressed & buttonMenu) == buttonMenu) ? 1 : ((lefControllerState.ulButtonPressed & buttonMenu) == buttonMenu) ? 0.5 : 0;
	output->LeftThumb = ((lefControllerState.ulButtonPressed & buttonThumb) == buttonThumb) ? 1 : ((lefControllerState.ulButtonPressed & buttonThumb) == buttonThumb) ? 0.5 : 0;

	vr::TrackedDevicePose_t rightTrackedDevicePose;
	vr::VRControllerState_t rightControllerState;
	m_system->GetControllerStateWithPose(vr::TrackingUniverseStanding, rightID, &rightControllerState, sizeof(rightControllerState), &rightTrackedDevicePose);
	ovr_freepie_setPose(&rightTrackedDevicePose, &output->rightHand);
	output->StatusRightHand = rightTrackedDevicePose.eTrackingResult == vr::TrackingResult_Running_OK ? 2 : rightTrackedDevicePose.eTrackingResult > vr::TrackingResult_Running_OK ? 1 : 0;
	output->RightTrigger = rightControllerState.rAxis[triggerID].x;
	output->RightGrip = rightControllerState.rAxis[gripID].x;
	output->RightStickAxes[0] = rightControllerState.rAxis[stickID].x;
	output->RightStickAxes[1] = rightControllerState.rAxis[stickID].y;

	output->A = ((rightControllerState.ulButtonPressed & buttonA) == buttonA) ? 1 : ((rightControllerState.ulButtonPressed & buttonA) == buttonA) ? 0.5 : 0;
	output->B = ((rightControllerState.ulButtonPressed & buttonB) == buttonB) ? 1 : ((rightControllerState.ulButtonPressed & buttonB) == buttonB) ? 0.5 : 0;
	output->RightStick = ((rightControllerState.ulButtonPressed & buttonStick) == buttonStick) ? 1 : ((rightControllerState.ulButtonPressed & buttonStick) == buttonStick) ? 0.5 : 0;
	output->Home = ((rightControllerState.ulButtonPressed & buttonMenu) == buttonMenu) ? 1 : ((rightControllerState.ulButtonPressed & buttonMenu) == buttonMenu) ? 0.5 : 0;
	output->RightThumb = ((rightControllerState.ulButtonPressed & buttonThumb) == buttonThumb) ? 1 : ((rightControllerState.ulButtonPressed & buttonThumb) == buttonThumb) ? 0.5 : 0;

	output->HmdMounted = m_system->GetTrackedDeviceActivityLevel(0) == vr::k_EDeviceActivityLevel_UserInteraction;
	
	return 0;
}
int ovr_freepie_trigger_haptic_pulse(unsigned int controllerIndex, unsigned int durationMicroSec, float frequency, float amplitude)
{
	if (controllerIndex == 0)
	{
		if (leftID)
		{
			m_system->TriggerHapticPulse(leftID, 0, (unsigned short)(std::min(50000.0f, durationMicroSec * amplitude)));
			return 1;
		}
	}
	else
	{
		if (rightID)
		{
			m_system->TriggerHapticPulse(rightID, 0, (unsigned short)(std::min(50000.0f, durationMicroSec * amplitude)));
			return 2;
		}
	}

	return 0;
}

int ovr_freepie_destroy()
{
	vr::VR_Shutdown();

	return 0;
}