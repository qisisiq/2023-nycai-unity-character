using UnityEngine;

public class MoveObjectTestCode:MonoBehaviour
{
	private PerfectLookAt m_PerfectLookAt;
	private  GUIStyle m_Style;
	private Animator m_Anim;
	private bool m_switch = true;

	private void Start()
	{
		m_Style = new GUIStyle();
		m_PerfectLookAt = GetComponent<PerfectLookAt>();
		m_Anim = GetComponent<Animator>();
		m_Style.normal.textColor = new Color(0.5f, 0, 0.75f, 1);
		m_Style.fontSize = 20;
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(0, 0, 100, 100), "Use W, A, S, D, Q and E to the target object. Use O to activate/deactivate Perfect Look At\nHold T or G to blend between idle, walk and run", m_Style);
	}

	private void Update()
	{
		if (m_Anim != null) {
			float currentSpeed = m_Anim.GetFloat("speed");

			if (Input.GetKey(KeyCode.T)) {
				currentSpeed += 2.0f * Time.deltaTime;
				currentSpeed = Mathf.Clamp(currentSpeed, 0.0f, 2.0f);
				m_Anim.SetFloat("speed", currentSpeed);
			}

			if (Input.GetKey(KeyCode.G)) {
				currentSpeed -= 2.0f * Time.deltaTime;
				currentSpeed = Mathf.Clamp(currentSpeed, 0.0f, 2.0f);
				m_Anim.SetFloat("speed", currentSpeed);
			}
		}

		if (m_PerfectLookAt != null) {
			GameObject perfectLookAtTarget = m_PerfectLookAt.m_TargetObject;

			if (perfectLookAtTarget != null) {
				if (Input.GetKeyDown(KeyCode.O)) {
					m_switch = !m_switch;
					if (m_switch) { m_PerfectLookAt.EnablePerfectLookat(0.4f); }
					else { m_PerfectLookAt.DisablePerfectLookat(0.4f); }
				}

				if (Input.GetKey(KeyCode.A)) {
					perfectLookAtTarget.transform.position = new Vector3(perfectLookAtTarget.transform.position.x + Time.deltaTime,
						perfectLookAtTarget.transform.position.y,
						perfectLookAtTarget.transform.position.z);
				}

				if (Input.GetKey(KeyCode.D)) {
					perfectLookAtTarget.transform.position = new Vector3(perfectLookAtTarget.transform.position.x - Time.deltaTime,
						perfectLookAtTarget.transform.position.y,
						perfectLookAtTarget.transform.position.z);
				}

				if (Input.GetKey(KeyCode.W)) {
					perfectLookAtTarget.transform.position = new Vector3(perfectLookAtTarget.transform.position.x,
						perfectLookAtTarget.transform.position.y + Time.deltaTime,
						perfectLookAtTarget.transform.position.z);
				}

				if (Input.GetKey(KeyCode.S)) {
					perfectLookAtTarget.transform.position = new Vector3(perfectLookAtTarget.transform.position.x,
						perfectLookAtTarget.transform.position.y - Time.deltaTime,
						perfectLookAtTarget.transform.position.z);
				}

				if (Input.GetKey(KeyCode.Q)) {
					perfectLookAtTarget.transform.position = new Vector3(perfectLookAtTarget.transform.position.x,
						perfectLookAtTarget.transform.position.y,
						perfectLookAtTarget.transform.position.z - Time.deltaTime);
				}

				if (Input.GetKey(KeyCode.E)) {
					perfectLookAtTarget.transform.position = new Vector3(perfectLookAtTarget.transform.position.x,
						perfectLookAtTarget.transform.position.y,
						perfectLookAtTarget.transform.position.z + Time.deltaTime);
				}
			}
		}
	}
}