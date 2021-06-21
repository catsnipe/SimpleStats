using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

/// <summary>
/// Measure the processing time of the loop script that Unity executes every frame.
/// 毎フレーム実行するスクリプトの処理時間計測
/// http://tsubakit1.hateblo.jp/entry/2018/04/17/233000
/// </summary>
public class ScriptCounter
{
    // event definition
    public struct UpdateTimeStart { }
    public struct UpdateTimeEnd { }

    public class LoopsInfo
    {
        public string	LoopName;
        public int		FrameCount;
        public float	PastTime;
        public float	TotalTime;
        public float	AverageTime;
    }
    
    static List<LoopsInfo> loops;
    static float time;
    
    /// <summary>
    /// Insert a unique event at the beginning and end of the loop event.
    /// ループイベントの最初と最後に独自イベントを挟む
    /// </summary>
    [RuntimeInitializeOnLoadMethod()]
    static void Initialize()
    {
        var playerLoop = UnityEngine.LowLevel.PlayerLoop.GetDefaultPlayerLoop();
        
        loops = new List<LoopsInfo>();
        foreach (var loop in playerLoop.subSystemList)
        {
            LoopsInfo group = new LoopsInfo();
            group.LoopName  = loop.type.Name;

            loops.Add(group);
        }
        
        // 計測開始
        UnityEngine.LowLevel.PlayerLoopSystem presys = new UnityEngine.LowLevel.PlayerLoopSystem()
        {
            type = typeof(UpdateTimeStart),
            updateDelegate = () => { StartMeasure(); }
        };
        // 計測終了
        UnityEngine.LowLevel.PlayerLoopSystem[] postsys = new UnityEngine.LowLevel.PlayerLoopSystem[]
        {
            new UnityEngine.LowLevel.PlayerLoopSystem()
            {
                type = typeof(UpdateTimeEnd),
                updateDelegate = () => { EndMeasure(0); }
            },
            new UnityEngine.LowLevel.PlayerLoopSystem()
            {
                type = typeof(UpdateTimeEnd),
                updateDelegate = () => { EndMeasure(1); }
            },
            new UnityEngine.LowLevel.PlayerLoopSystem()
            {
                type = typeof(UpdateTimeEnd),
                updateDelegate = () => { EndMeasure(2); }
            },
            new UnityEngine.LowLevel.PlayerLoopSystem()
            {
                type = typeof(UpdateTimeEnd),
                updateDelegate = () => { EndMeasure(3); }
            },
            new UnityEngine.LowLevel.PlayerLoopSystem()
            {
                type = typeof(UpdateTimeEnd),
                updateDelegate = () => { EndMeasure(4); }
            },
            new UnityEngine.LowLevel.PlayerLoopSystem()
            {
                type = typeof(UpdateTimeEnd),
                updateDelegate = () => { EndMeasure(5); }
            },
            new UnityEngine.LowLevel.PlayerLoopSystem()
            {
                type = typeof(UpdateTimeEnd),
                updateDelegate = () => { EndMeasure(6); }
            },
        };
        
        if (playerLoop.subSystemList.Length != postsys.Length)
        {
            Debug.LogError($"unity in current version is unsupported.");
            return;
        }

        // In Unity 2019.4
        //
        // initialization
        // earlyupdate
        // fixedupdate
        // preupdate
        // update
        // prelateupdate
        // postlateupdate
        for (int group = 0; group < playerLoop.subSystemList.Length; group++)
        {
            var updateSystem = playerLoop.subSystemList[group];
            var subSystem    = new List<UnityEngine.LowLevel.PlayerLoopSystem>(updateSystem.subSystemList);

            subSystem.Insert(0, presys);
            subSystem.Add(postsys[group]);
    
            updateSystem.subSystemList      = subSystem.ToArray();
            playerLoop.subSystemList[group] = updateSystem;
        }
        
        UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(playerLoop);
    }
    
    /// <summary>
    /// Get information for each group of PlayerLoops.
    /// PlayerLoops のグループごとの情報を取得
    /// </summary>
    public static List<LoopsInfo> GetPlayerLoopsInfo()
    {
        return loops;
    }
    
    /// <summary>
    /// Start measurement.
    /// 計測開始
    /// </summary>
    static void StartMeasure()
    {
        time = Time.realtimeSinceStartup;
    }
    
    /// <summary>
    /// End measurement.
    /// 計測終了
    /// </summary>
    /// <param name="loopsNo">PlayerLoops group number</param>
    static void EndMeasure(int loopsNo)
    {
        LoopsInfo group = loops[loopsNo];
        
        group.FrameCount++;
        group.PastTime     = Time.realtimeSinceStartup - time;
        group.TotalTime   += group.PastTime;
        group.AverageTime  = group.TotalTime / group.FrameCount;
    }
    
}
