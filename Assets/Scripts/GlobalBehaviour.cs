// <copyright file="GlobalBehaviour.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using UnityEngine;

[DisallowMultipleComponent]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class GlobalBehaviour : MonoBehaviour
{
    protected ActivityLog log;
    protected GameController game;
    protected MapController map;
    protected PlayerList players;

    public static Transform GlobalParent;

    public static GameObject CreateResource(string name, Transform parent)
    {
        return CreateResource(name, Vector3.zero, Quaternion.identity, GlobalParent);
    }
    
    public static GameObject CreateResource(string name, Vector3? position, Quaternion? rotation)
    {
        return CreateResource(name, position, rotation, GlobalParent);
    }

    public static GameObject CreateResource(string name, Vector3? position, Transform parent)
    {
        return CreateResource(name, position, Quaternion.identity, parent);
    }

    public static GameObject CreateResource(string name, Vector3? position, Quaternion? rotation, Transform parent)
    {
        var prefab = Resources.Load("Tile_" + name, typeof(GameObject));

        if (prefab == null)
        {
            Debug.LogWarning("Something about this project has been corrupted.");
            Debug.LogError("Unable to load: Prefabs/Map/Resources/Tile_" + name);
            return null;
        }

        GameObject go = Instantiate(prefab, position ?? Vector3.zero, rotation ?? Quaternion.identity) as GameObject;
        go.transform.parent = parent;
        go.name = prefab.name;
        return go;
    }

    /// <summary>
    /// UnityEngine.MonoBehaviour.Awake
    /// specifically overwritten by things that implement us
    /// </summary>
    protected virtual void Awake()
    {
        log = GetComponent<ActivityLog>();
        game = GetComponent<GameController>();
        map = GetComponent<MapController>();
        players = GetComponent<PlayerList>();

        if (log == null ||
            game == null ||
            map == null ||
            players == null)
        {
            GameObject go = GameObject.Find("Control Objects");
            log = go.GetComponent<ActivityLog>();
            map = go.GetComponent<MapController>();
            players = go.GetComponent<PlayerList>();

            game = go.transform.Find("Game").GetComponent<GameController>();

            GlobalParent = GameObject.Find("Global Parent").transform;
        }
    }
}
