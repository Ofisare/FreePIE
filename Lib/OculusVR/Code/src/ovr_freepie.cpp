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
	output->statusHead = headTrackedDevicePose.eTrackingResult;

	vr::TrackedDevicePose_t leftTrackedDevicePose;
	vr::VRControllerState_t lefControllerState;
	m_system->GetControllerStateWithPose(vr::TrackingUniverseStanding, leftID, &lefControllerState, sizeof(lefControllerState), &leftTrackedDevicePose);
	ovr_freepie_setPose(&leftTrackedDevicePose, &output->leftHand);
	output->statusLeftHand = leftTrackedDevicePose.eTrackingResult;
	output->LTrigger = lefControllerState.rAxis[triggerID].x;
	output->LGrip = lefControllerState.rAxis[gripID].x;
	output->Lstick[0] = lefControllerState.rAxis[stickID].x;
	output->Lstick[1] = lefControllerState.rAxis[stickID].y;
	output->Lbuttons = (unsigned int)lefControllerState.ulButtonPressed;
	output->Ltouches = (unsigned int)lefControllerState.ulButtonTouched;

	vr::TrackedDevicePose_t rightTrackedDevicePose;
	vr::VRControllerState_t rightControllerState;
	m_system->GetControllerStateWithPose(vr::TrackingUniverseStanding, rightID, &rightControllerState, sizeof(rightControllerState), &rightTrackedDevicePose);
	ovr_freepie_setPose(&rightTrackedDevicePose, &output->rightHand);
	output->statusRightHand = rightTrackedDevicePose.eTrackingResult;
	output->RTrigger = rightControllerState.rAxis[triggerID].x;
	output->RGrip = rightControllerState.rAxis[gripID].x;
	output->Rstick[0] = rightControllerState.rAxis[stickID].x;
	output->Rstick[1] = rightControllerState.rAxis[stickID].y;
	output->Rbuttons = (unsigned int)rightControllerState.ulButtonPressed;
	output->Rtouches = (unsigned int)rightControllerState.ulButtonTouched;

	output->HmdMounted = m_system->GetTrackedDeviceActivityLevel(0) == vr::k_EDeviceActivityLevel_UserInteraction;
	
	return 0;
}
int ovr_freepie_trigger_haptic_pulse(unsigned int controllerIndex, unsigned int axis, unsigned int durationMicroSec)
{
	if (controllerIndex == 0)
	{
		if (leftID)
		{
			m_system->TriggerHapticPulse(leftID, axis, (unsigned short)durationMicroSec);
			return 1;
		}
	}
	else
	{
		if (rightID)
		{
			m_system->TriggerHapticPulse(rightID, axis, (unsigned short)durationMicroSec);
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