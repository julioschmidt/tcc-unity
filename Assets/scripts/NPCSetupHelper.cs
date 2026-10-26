using UnityEngine;

// Script auxiliar para configuração automática de NPCs
// Anexar ao GameObject do NPC junto com o NPCModalTrigger
public class NPCSetupHelper : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    
    [Header("Collider Setup")]
    [SerializeField] private bool createTriggerCollider = true;
    [SerializeField] private float colliderRadius = 3f;
    
    [Header("Interaction Indicator Setup")]
    [SerializeField] private bool createIndicator = true;
    [SerializeField] private GameObject indicatorPrefab;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupNPC();
        }
    }
    
    [ContextMenu("Setup NPC")]
    public void SetupNPC()
    {
        NPCModalTrigger trigger = GetComponent<NPCModalTrigger>();
        
        if (trigger == null)
        {
            Debug.LogError("NPCModalTrigger não encontrado! Adicione o componente primeiro.");
            return;
        }
        
        // Configurar collider se necessário
        if (createTriggerCollider)
        {
            SetupCollider(trigger);
        }
        
        // Configurar indicador se necessário
        if (createIndicator)
        {
            SetupIndicator(trigger);
        }
        
        Debug.Log($"NPC {gameObject.name} configurado automaticamente!");
    }
    
    private void SetupCollider(NPCModalTrigger trigger)
    {
        // Verificar se já tem collider
        Collider existingCollider = GetComponent<Collider>();
        
        if (existingCollider == null)
        {
            // Criar um SphereCollider como trigger
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = colliderRadius;
            
            Debug.Log($"SphereCollider criado para {gameObject.name}");
        }
        else
        {
            // Configurar o collider existente como trigger
            existingCollider.isTrigger = true;
            Debug.Log($"Collider existente configurado como trigger para {gameObject.name}");
        }
    }
    
    private void SetupIndicator(NPCModalTrigger trigger)
    {
        // Verificar se já tem indicador
        if (trigger.interactionIndicator != null)
        {
            Debug.Log($"Indicador já existe para {gameObject.name}");
            return;
        }
        
        GameObject indicator;
        
        if (indicatorPrefab != null)
        {
            // Usar prefab personalizado
            indicator = Instantiate(indicatorPrefab, transform);
        }
        else
        {
            // Criar indicador simples
            indicator = CreateSimpleIndicator();
        }
        
        // Configurar no trigger
        trigger.interactionIndicator = indicator;
        
        Debug.Log($"Indicador criado para {gameObject.name}");
    }
    
    private GameObject CreateSimpleIndicator()
    {
        // Criar GameObject do indicador
        GameObject indicator = new GameObject("InteractionIndicator");
        indicator.transform.SetParent(transform);
        indicator.transform.localPosition = Vector3.up * 2f; // 2 metros acima do NPC
        
        // Adicionar um quad simples com material
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.SetParent(indicator.transform);
        quad.transform.localPosition = Vector3.zero;
        quad.transform.localScale = Vector3.one * 0.5f;
        
        // Remover collider do quad
        Collider quadCollider = quad.GetComponent<Collider>();
        if (quadCollider != null)
        {
            DestroyImmediate(quadCollider);
        }
        
        // Configurar material
        Renderer quadRenderer = quad.GetComponent<Renderer>();
        if (quadRenderer != null)
        {
            // Criar material simples
            Material indicatorMaterial = new Material(Shader.Find("Sprites/Default"));
            indicatorMaterial.color = Color.yellow;
            quadRenderer.material = indicatorMaterial;
        }
        
        // Adicionar animação simples (opcional)
        SimpleIndicatorAnimator animator = indicator.AddComponent<SimpleIndicatorAnimator>();
        
        return indicator;
    }
}

// Animador simples para o indicador
public class SimpleIndicatorAnimator : MonoBehaviour
{
    [Header("Animação")]
    public float floatSpeed = 1f;
    public float floatAmount = 0.3f;
    public float rotateSpeed = 30f;
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.localPosition;
    }
    
    void Update()
    {
        // Animação de flutuação
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
        
        // Rotação
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
        
        // Sempre olhar para a câmera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 lookDirection = mainCamera.transform.position - transform.position;
            lookDirection.y = 0; // Manter apenas rotação horizontal
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }
}

// Script para configuração global de NPCs na cena
public class NPCGlobalManager : MonoBehaviour
{
    [Header("Configuração Global")]
    public CustomVRModal sharedModal; // Modal compartilhado por todos os NPCs
    public Transform playerTransform; // Referência global do player
    
    [Header("Auto Find")]
    public bool findModalAutomatically = true;
    public bool findPlayerAutomatically = true;
    
    void Start()
    {
        SetupAllNPCs();
    }
    
    [ContextMenu("Setup All NPCs")]
    public void SetupAllNPCs()
    {
        // Encontrar modal e player automaticamente
        if (findModalAutomatically && sharedModal == null)
        {
            sharedModal = FindFirstObjectByType<CustomVRModal>();
        }
        
        if (findPlayerAutomatically && playerTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                playerTransform = mainCamera.transform;
            }
        }
        
        // Configurar todos os NPCs na cena
        NPCModalTrigger[] npcs = FindObjectsByType<NPCModalTrigger>(FindObjectsSortMode.None);
        
        foreach (NPCModalTrigger npc in npcs)
        {
            if (npc.vrModal == null)
            {
                npc.vrModal = sharedModal;
            }
            
            if (npc.playerTransform == null)
            {
                npc.playerTransform = playerTransform;
            }
        }
        
        Debug.Log($"Configurados {npcs.Length} NPCs na cena");
    }
}