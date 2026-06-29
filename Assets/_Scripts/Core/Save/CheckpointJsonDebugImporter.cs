//using System;
//using System.IO;
//using UnityEngine;

//public static class CheckpointJsonDebugExporter
//{
//    private const string FileNameFormat = "checkpoint_slot_{0}.json";

//    public static bool Export(int slotIndex, CheckpointSaveData data)
//    {
//        if (slotIndex < 0 || slotIndex >= CheckpointSaveSystem.SlotCount)
//        {
//            Debug.LogError(
//                $"CheckpointJsonDebugExporter: неверный индекс слота {slotIndex}. " +
//                $"Допустимый диапазон: 0..{CheckpointSaveSystem.SlotCount - 1}.");
//            return false;
//        }

//        if (data == null)
//        {
//            Debug.LogError("CheckpointJsonDebugExporter.Export: data == null.");
//            return false;
//        }

//        try
//        {
//            string fileName = string.Format(FileNameFormat, slotIndex);
//            string path = Path.Combine(Application.persistentDataPath, fileName);
//            string json = JsonUtility.ToJson(data, true);

//            File.WriteAllText(path, json);
//            Debug.Log($"Checkpoint JSON written: {path}");
//            return true;
//        }
//        catch (Exception exception)
//        {
//            Debug.LogError($"CheckpointJsonDebugExporter: ошибка записи JSON для слота {slotIndex}: {exception.Message}");
//            return false;
//        }
//    }
//}