// <copyright file="Database.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
#if UNITY_EDITOR
using System.Collections;
using System.IO;
#endif

using SQLite4Unity3d;

/// <summary>
/// 
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class Database
{
    private SQLiteConnection connection;

    public static SQLiteConnection GetConnection(string databaseName)
    {
#if UNITY_EDITOR
        var dbPath = string.Format(@"Assets/StreamingAssets/{0}", databaseName);
#else
        // check if file exists in Application.persistentDataPath
        var filepath = string.Format("{0}/{1}", Application.persistentDataPath, databaseName);

        if (!File.Exists(filepath))
        {
            Debug.Log("Database not in Persistent path");
            
	        var loadDb = Application.dataPath + "/StreamingAssets/" + databaseName;
            
	        // then save to Application.persistentDataPath
	        File.Copy(loadDb, filepath);
            Debug.Log("Database written");
        }

        var dbPath = filepath;
#endif
        return new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite);
    }
}
