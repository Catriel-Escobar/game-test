using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class EmbestidaTrial : MonoBehaviour
{
    public float activeTime = 2f;

    [Header("Mesh Related")]
    public float meshRefreshRate = 0.1f;
    public float meshDestoyDelay = 1f;
    public Transform postionToSpawn;

    [Header("Shader Related")]
    public Material mat;
    public string shaderVarRef;
    public float shaderVarRate = 0.1f;
    public float shaderVarRefreshRate = 0.05f;

    [Header("Rotation")]
    public Vector3 rotationOffset;

    private bool isTrailActive;
    private Coroutine _trailRoutine;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private int _shaderVarId;
    private bool _shaderVarValid;
    private Vector3 _trailDirection = Vector3.forward;

    private void Awake()
    {
        _shaderVarValid = !string.IsNullOrWhiteSpace(shaderVarRef);

        if (_shaderVarValid)
        {
            _shaderVarId = Shader.PropertyToID(shaderVarRef);
        }
    }

    public void PlayTrail()
    {
        PlayTrail(transform.forward);
    }

    public void PlayTrail(Vector3 trailDirection)
    {
        if (isTrailActive || _trailRoutine != null)
            return;

        if (mat == null)
        {
            Debug.LogWarning($"[EmbestidaTrail] Falta asignar un material en {name}.");
            return;
        }

        _trailDirection = trailDirection;
        _trailRoutine = StartCoroutine(Embestida(activeTime));
    }

    public void StopTrail()
    {
        if (_trailRoutine != null)
        {
            StopCoroutine(_trailRoutine);
            _trailRoutine = null;
        }

        isTrailActive = false;
    }

    private IEnumerator Embestida(float timeActive)
    {
        isTrailActive = true;

        while (timeActive > 0f)
        {
            timeActive -= meshRefreshRate;

            if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
            {
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            }

            Transform spawnTransform = postionToSpawn != null ? postionToSpawn : transform;
            Quaternion spawnRotation = GetFlatRotation(_trailDirection) * Quaternion.Euler(rotationOffset);

            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                SkinnedMeshRenderer skinnedMeshRenderer = skinnedMeshRenderers[i];
                if (skinnedMeshRenderer == null)
                    continue;

                GameObject gObj = new GameObject($"{name}_TrailGhost_{i}");
                gObj.transform.SetPositionAndRotation(spawnTransform.position, spawnRotation);

                MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
                MeshFilter mf = gObj.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                skinnedMeshRenderer.BakeMesh(mesh);

                mf.sharedMesh = mesh;
                mr.material = new Material(mat);
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.receiveShadows = false;

                if (_shaderVarValid)
                {
                    StartCoroutine(AnimateMaterialFloat(mr.material, 1f, 0f, shaderVarRate));
                }

                Destroy(mesh, meshDestoyDelay);
                Destroy(gObj, meshDestoyDelay);
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }

        isTrailActive = false;
        _trailRoutine = null;
    }

    private IEnumerator AnimateMaterialFloat(Material materialInstance, float startValue, float targetValue, float step)
    {
        if (materialInstance == null || !_shaderVarValid)
            yield break;

        float currentValue = startValue;
        materialInstance.SetFloat(_shaderVarId, currentValue);

        while (!Mathf.Approximately(currentValue, targetValue))
        {
            currentValue = Mathf.MoveTowards(currentValue, targetValue, Mathf.Abs(step));
            materialInstance.SetFloat(_shaderVarId, currentValue);
            yield return new WaitForSeconds(shaderVarRefreshRate);
        }
    }

    private Quaternion GetFlatRotation(Vector3 direction)
    {
        Vector3 forward = direction;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }

        return Quaternion.LookRotation(forward.normalized, Vector3.up);
    }

    private void OnDisable()
    {
        StopTrail();
    }
}
