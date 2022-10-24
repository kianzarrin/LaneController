namespace LaneConroller.LifeCycle;
using ICities;
using KianCommons;
using LaneConroller.Manager;
using System;

public class SerializableDataExtension : SerializableDataExtensionBase {
    const string DATA_ID = "LaneConroller";
    SerializableDataExtension Instance;
    public override void OnCreated(ISerializableData serializableData) => Instance = this;
    public override void OnReleased() => Instance = null;
    private static ISerializableData SerializableData => SimulationManager.instance.m_SerializableDataWrapper;

    public override void OnLoadData() => Load();
    public static void Load() {
        Log.Called();
        try {
            Log.Called();
            byte[] data = SerializableData.LoadData(DATA_ID);
            LaneConrollerManager.Deserialize(data);
            Log.Succeeded();
        } catch (Exception ex) { ex.Log(); }
    }

    public override void OnSaveData() => Save();
    public static void Save() {
        Log.Called();
        try {
            Log.Called();
            var data = LaneConrollerManager.Instance.Serialize();
            if (data != null) {
                SerializableData.SaveData(DATA_ID, data);
            }
            Log.Succeeded();
        } catch (Exception ex) { ex.Log(); }
    }
}
