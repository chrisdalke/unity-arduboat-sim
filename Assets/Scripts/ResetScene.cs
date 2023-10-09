using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResetScene : MonoBehaviour
{
    private void FixedUpdate() {
        if (Input.GetKey("r")) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
