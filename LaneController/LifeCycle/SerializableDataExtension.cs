namespace LaneConroller.LifeCycle;
using ICities;
using KianCommons;
using LaneConroller.Manager;
using System;

public class SerializableDataExtension : SerializableDataExtensionBase {
    const string DATA_ID = "LaneConroller";
    public override void OnLoadData() {
        try {
            Log.Called();
            byte[] data = serializableDataManager.LoadData(DATA_ID);
            LaneConrollerManager.Deserialize(data);
            Log.Succeeded();
        } catch (Exception ex) { ex.Log(); }
    }
    public override void OnSaveData() {
        try {
            Log.Called();
            var data = LaneConrollerManager.Instance.Serialize();
            if (data != null) {
                serializableDataManager.SaveData(DATA_ID, data);
            }
            Log.Succeeded();
        } catch (Exception ex) { ex.Log(); }
    }
}
