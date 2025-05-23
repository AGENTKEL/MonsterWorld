using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionSphere : MonoBehaviour {
    
    public float maxRadius = 10f;
    public float growSpeed = 40f;
    public float fadeSpeed = 20f;
    public float minDistance = 30f;
    public float interactionCooldown = 1.5f;

    public Material sphereMaterial;

    private GameObject sphere;
    private float currentRadius = 0f;
    private float currentAlpha = 0.5f;
    private bool isGrowing = false;
    private bool hasInteracted = false;

    [Header("Stats")] 
    public int sphereDamage = 10;

    [SerializeField] private LevelSystem _levelSystem;

    void Update() {
        if (isGrowing) UpdateSphere();
        
        string[] tagsToCheck = { "Food", "Enemy", "Buy", "Chest"};

        foreach (string tag in tagsToCheck) {
            GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject target in targets) {
                if (Vector3.Distance(transform.position, target.transform.position) <= minDistance) {
                    break;
                }
            }
        }
    }

    public void StartInteractionSphere(int damage) {
        if (sphere != null) Destroy(sphere);
        sphereDamage = damage;
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = transform.position;
        sphere.transform.localScale = Vector3.zero;
        Destroy(sphere.GetComponent<Collider>());
        

        sphereMaterial.SetFloat("_Surface", 1);
        sphereMaterial.SetFloat("_Mode", 3);
        sphereMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        sphereMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        sphereMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        sphereMaterial.SetInt("_ZWrite", 0);
        sphereMaterial.DisableKeyword("_ALPHATEST_ON");
        sphereMaterial.EnableKeyword("_ALPHABLEND_ON");
        sphereMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        sphereMaterial.renderQueue = 3000;

        sphere.GetComponent<Renderer>().material = sphereMaterial;
        Color color = sphereMaterial.color;
        color.a = currentAlpha;
        sphereMaterial.color = color;

        currentRadius = 0f;
        isGrowing = true;
    }

    void UpdateSphere() {
        currentRadius += growSpeed * Time.deltaTime;
        sphere.transform.localScale = Vector3.one * currentRadius;

        currentAlpha = Mathf.Lerp(currentAlpha, 0f, fadeSpeed * Time.deltaTime);
        Color color = sphereMaterial.color;
        color.a = Mathf.Lerp(0.5f, currentAlpha, currentRadius / maxRadius);
        sphereMaterial.color = color;

        if (currentRadius >= maxRadius) {
            Destroy(sphere);
            isGrowing = false;
        }

        CheckForInteractions();
    }

    void CheckForInteractions() {
        if (hasInteracted) return;

        Collider[] colliders = Physics.OverlapSphere(sphere.transform.position, currentRadius);
        foreach (Collider col in colliders) {
            if (col.CompareTag("Food")) {
                col.GetComponent<Food>()?.TakeDamage(sphereDamage);
                hasInteracted = true;
                StartCoroutine(ResetInteraction());
                break;
            }
            
            if (col.CompareTag("Enemy")) {
                col.GetComponent<Enemy>()?.TakeDamage(sphereDamage);
                hasInteracted = true;
                StartCoroutine(ResetInteraction());
                break;
            }
            
            if (col.CompareTag("Buy")) {
                col.GetComponent<MoneyInteract>()?.Interact(_levelSystem);
                hasInteracted = true;
                StartCoroutine(ResetInteraction());
                break;
            }
            
            if (col.CompareTag("Chest")) {
                col.GetComponent<Chest>()?.Interact(_levelSystem);
                hasInteracted = true;
                StartCoroutine(ResetInteraction());
                break;
            }
        }
    }

    IEnumerator ResetInteraction() {
        yield return new WaitForSeconds(interactionCooldown);
        hasInteracted = false;
    }
}
