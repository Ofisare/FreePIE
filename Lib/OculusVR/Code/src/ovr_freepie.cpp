extern "C"
{
  #include "../include/ovr_freepie.h"
}
#include <openxr.h>
#include <string.h>

constexpr static XrFormFactor m_formFactor{XR_FORM_FACTOR_HEAD_MOUNTED_DISPLAY};

XrInstance m_instance;
XrSystemId m_system;
XrSession m_session;

XrActionSet m_actionSet;
XrAction m_placeAction;
XrAction m_exitAction;
XrAction m_poseAction;
XrAction m_vibrateAction;

constexpr static uint32_t LeftSide = 0;
constexpr static uint32_t RightSide = 1;

constexpr static uint32_t SubactionCount = 2;
XrPath m_subactionPaths[SubactionCount];

constexpr static uint32_t BindingCount = 8;
XrActionSuggestedBinding m_bindings[BindingCount];

unsigned int leftID;
unsigned int rightID;
unsigned int triggerID;
unsigned int gripID;
unsigned int stickID;

double	HmdFrameTiming;

int ovr_freepie_init()
{
	XrInstanceCreateInfo instanceInfo{ XR_TYPE_INSTANCE_CREATE_INFO };
	instanceInfo.applicationInfo = { "BasicXrApp", 1, "", 1, XR_CURRENT_API_VERSION };
	XrResult result = xrCreateInstance(&instanceInfo, &m_instance);
	if (XR_SUCCEEDED(result) == false)
		return 1;

	XrSystemGetInfo systemInfo{ XR_TYPE_SYSTEM_GET_INFO };
	systemInfo.formFactor = m_formFactor;
	result = xrGetSystem(m_instance, &systemInfo, &m_system);
	if (XR_SUCCEEDED(result) == false)
		return 1;

	XrSessionCreateInfo sessionInfo{ XR_TYPE_SESSION_CREATE_INFO };
	sessionInfo.systemId = m_system;
	result = xrCreateSession(m_instance, &sessionInfo, &m_session);
	if (XR_SUCCEEDED(result) == false)
		return 1;

	// Create an action set.
	{
		XrActionSetCreateInfo actionSetInfo{ XR_TYPE_ACTION_SET_CREATE_INFO };
		strcpy_s(actionSetInfo.actionSetName, "default");
		strcpy_s(actionSetInfo.localizedActionSetName, "default");
		result = xrCreateActionSet(m_instance, &actionSetInfo, &m_actionSet);
		if (XR_SUCCEEDED(result) == false) return 1;
	}

	// Create actions.
	{
		// Enable subaction path filtering for left or right hand.
		xrStringToPath(m_instance, "/user/hand/left", &m_subactionPaths[LeftSide]);
		xrStringToPath(m_instance, "/user/hand/right", &m_subactionPaths[RightSide]);

		// Create an input action to place a hologram.
		{
			XrActionCreateInfo actionInfo{ XR_TYPE_ACTION_CREATE_INFO };
			actionInfo.actionType = XR_ACTION_TYPE_BOOLEAN_INPUT;
			strcpy_s(actionInfo.actionName, "select");
			strcpy_s(actionInfo.localizedActionName, "select");
			actionInfo.countSubactionPaths = SubactionCount;
			actionInfo.subactionPaths = m_subactionPaths;
			result = xrCreateAction(m_actionSet, &actionInfo, &m_placeAction);
			if (XR_SUCCEEDED(result) == false) return 1;
		}

		// Create an input action getting the left and right hand poses.
		{
			XrActionCreateInfo actionInfo{ XR_TYPE_ACTION_CREATE_INFO };
			actionInfo.actionType = XR_ACTION_TYPE_POSE_INPUT;
			strcpy_s(actionInfo.actionName, "hand_pose");
			strcpy_s(actionInfo.localizedActionName, "Hand Pose");
			actionInfo.countSubactionPaths = SubactionCount;
			actionInfo.subactionPaths = m_subactionPaths;
			result = xrCreateAction(m_actionSet, &actionInfo, &m_poseAction);
			if (XR_SUCCEEDED(result) == false) return 1;
		}

		// Create an output action for vibrating the left and right controller.
		{
			XrActionCreateInfo actionInfo{ XR_TYPE_ACTION_CREATE_INFO };
			actionInfo.actionType = XR_ACTION_TYPE_VIBRATION_OUTPUT;
			strcpy_s(actionInfo.actionName, "vibrate");
			strcpy_s(actionInfo.localizedActionName, "Vibrate");
			actionInfo.countSubactionPaths = SubactionCount;
			actionInfo.subactionPaths = m_subactionPaths;
			result = xrCreateAction(m_actionSet, &actionInfo, &m_vibrateAction);
			if (XR_SUCCEEDED(result) == false) return 1;
		}

		// Create an input action to exit the session.
		{
			XrActionCreateInfo actionInfo{ XR_TYPE_ACTION_CREATE_INFO };
			actionInfo.actionType = XR_ACTION_TYPE_BOOLEAN_INPUT;
			strcpy_s(actionInfo.actionName, "menu");
			strcpy_s(actionInfo.localizedActionName, "Menu");
			actionInfo.countSubactionPaths = SubactionCount;
			actionInfo.subactionPaths = m_subactionPaths;
			result = xrCreateAction(m_actionSet, &actionInfo, &m_exitAction);
			if (XR_SUCCEEDED(result) == false) return 1;
		}
	}

	// Set up suggested bindings for the simple_controller profile.
	{
		int currentIndex = 0;
		m_bindings[currentIndex].action = m_placeAction;
		xrStringToPath(m_instance, "/user/hand/right/input/select/click", &m_bindings[currentIndex++].binding);
		m_bindings[currentIndex].action = m_placeAction;
		xrStringToPath(m_instance, "/user/hand/left/input/select/click", &m_bindings[currentIndex++].binding);

		m_bindings[currentIndex].action = m_poseAction;
		xrStringToPath(m_instance, "/user/hand/right/input/grip/pose", &m_bindings[currentIndex++].binding);
		m_bindings[currentIndex].action = m_poseAction;
		xrStringToPath(m_instance, "/user/hand/left/input/grip/pose", &m_bindings[currentIndex++].binding);

		m_bindings[currentIndex].action = m_vibrateAction;
		xrStringToPath(m_instance, "/user/hand/right/output/haptic", &m_bindings[currentIndex++].binding);
		m_bindings[currentIndex].action = m_vibrateAction;
		xrStringToPath(m_instance, "/user/hand/left/output/haptic", &m_bindings[currentIndex++].binding);

		m_bindings[currentIndex].action = m_exitAction;
		xrStringToPath(m_instance, "/user/hand/right/input/menu/click", &m_bindings[currentIndex++].binding);
		m_bindings[currentIndex].action = m_exitAction;
		xrStringToPath(m_instance, "/user/hand/left/input/menu/click", &m_bindings[currentIndex++].binding);

		XrInteractionProfileSuggestedBinding suggestedBindings{ XR_TYPE_INTERACTION_PROFILE_SUGGESTED_BINDING };
		xrStringToPath(m_instance, "/user/hand/left/input/menu/click", &suggestedBindings.interactionProfile);
		suggestedBindings.suggestedBindings = m_bindings;
		suggestedBindings.countSuggestedBindings = BindingCount;
		result = xrSuggestInteractionProfileBindings(m_instance, &suggestedBindings);
		if (XR_SUCCEEDED(result) == false) return 1;
	}

	return 0;
}

int ovr_freepie_destroy()
{
	if (m_instance)
		xrDestroyInstance(m_instance);

	return 0;
}

int ovr_freepie_reset_orientation()
{
	//ovr_RecenterTrackingOrigin(HMD);
	return 0;
}

int ovr_freepie_setPose(XrPosef* pose, ovr_freepie_6dof *dof)
{
	/*vr::HmdMatrix34_t matrix = pose->mDeviceToAbsoluteTracking;

	dof->left[0] = matrix.m[0][0];
	dof->left[1] = matrix.m[1][0];
	dof->left[2] = matrix.m[2][0];

	dof->up[0] = matrix.m[0][1];
	dof->up[1] = matrix.m[1][1];
	dof->up[2] = matrix.m[2][1];

	dof->forward[0] = matrix.m[0][2];
	dof->forward[1] = matrix.m[1][2];
	dof->forward[2] = matrix.m[2][2];*/

	dof->position[0] = pose->position.x;
	dof->position[1] = pose->position.y;
	dof->position[2] = pose->position.z;
	return  0;
}

int ovr_freepie_read(ovr_freepie_data *output)
{
	XrActiveActionSet activeActionSets[1] = { {m_actionSet, XR_NULL_PATH} };
	XrActionsSyncInfo syncInfo{ XR_TYPE_ACTIONS_SYNC_INFO };

	syncInfo.countActiveActionSets = 1;
	syncInfo.activeActionSets = activeActionSets;
	XrResult result = xrSyncActions(m_session, &syncInfo);
	if (XR_SUCCEEDED(result) == false) return 1;

	XrActionStateBoolean placeActionValue{ XR_TYPE_ACTION_STATE_BOOLEAN };
	{
		XrActionStateGetInfo getInfo{ XR_TYPE_ACTION_STATE_GET_INFO };
		getInfo.action = m_placeAction;
		getInfo.subactionPath = m_subactionPaths[LeftSide];
		result = xrGetActionStateBoolean(m_session, &getInfo, &placeActionValue);
		if (XR_SUCCEEDED(result) == false) return 1;

		output->HmdMounted = placeActionValue.isActive && placeActionValue.currentState ? 1 : 0;
	}

	/*if (!leftID)
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

	output->HmdMounted = m_system->GetTrackedDeviceActivityLevel(0) == vr::k_EDeviceActivityLevel_UserInteraction;*/
	
	return 0;
}
int ovr_freepie_trigger_haptic_pulse(unsigned int controllerIndex, unsigned int axis, unsigned int durationMicroSec)
{
	/*std::vector<XrActiveActionSet> activeActionSets = {{m_actionSet.Get(), XR_NULL_PATH}};
	XrActionsSyncInfo syncInfo{ XR_TYPE_ACTIONS_SYNC_INFO };
	syncInfo.countActiveActionSets = (uint32_t)activeActionSets.size();
	syncInfo.activeActionSets = activeActionSets.data();
	CHECK_XRCMD(xrSyncActions(m_session.Get(), &syncInfo));

	// Check the state of the actions for left and right hands separately.
	for (uint32_t side : {LeftSide, RightSide}) {
		const XrPath subactionPath = m_subactionPaths[side];

		// Apply a tiny vibration to the corresponding hand to indicate that action is detected.
		auto ApplyVibration = [this, subactionPath] {
			XrHapticActionInfo actionInfo{ XR_TYPE_HAPTIC_ACTION_INFO };
			actionInfo.action = m_vibrateAction.Get();
			actionInfo.subactionPath = subactionPath;

			XrHapticVibration vibration{ XR_TYPE_HAPTIC_VIBRATION };
			vibration.amplitude = 0.5f;
			vibration.duration = XR_MIN_HAPTIC_DURATION;
			vibration.frequency = XR_FREQUENCY_UNSPECIFIED;
			CHECK_XRCMD(xrApplyHapticFeedback(m_session.Get(), &actionInfo, (XrHapticBaseHeader*)&vibration));
		};*/

	if (controllerIndex == 0)
	{
		if (leftID)
		{
			//m_system->TriggerHapticPulse(leftID, axis, (unsigned short)durationMicroSec);
			return 1;
		}
	}
	else
	{
		if (rightID)
		{
			//m_system->TriggerHapticPulse(rightID, axis, (unsigned short)durationMicroSec);
			return 2;
		}
	}

	return 0;
}