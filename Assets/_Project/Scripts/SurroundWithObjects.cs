using UnityEngine;

public class SurroundWithObjects : MonoBehaviour
{
    [SerializeField] GameObject surroundingObject;
    [SerializeField] float spacing = 0.01f;
    [SerializeField] float padding = 0f;

    void Start()
    {
        Surround();
    }

    void Surround()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("No Renderer found!");
            return;
        }

        Bounds bounds = rend.bounds;

        Vector3 size = bounds.size + Vector3.one * padding * 2f;

        int xCount = Mathf.CeilToInt(size.x / spacing);
        int yCount = Mathf.CeilToInt(size.y / spacing);
        int zCount = Mathf.CeilToInt(size.z / spacing);

        Vector3 start = bounds.center - size / 2f;

        for (int x = 0; x <= xCount; x++)
        {
            for (int y = 0; y <= yCount; y++)
            {
                for (int z = 0; z <= zCount; z++)
                {
                    bool isSurface =
                        x == 0 || x == xCount ||
                        y == 0 || y == yCount ||
                        z == 0 || z == zCount;

                    if (isSurface)
                    {
                        Vector3 pos = start + new Vector3(x * spacing, y * spacing, z * spacing);

                        Instantiate(surroundingObject, pos, Quaternion.identity, transform);
                    }
                }
            }
        }
    }
}