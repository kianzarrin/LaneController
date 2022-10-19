namespace PathController.LifeCycle;
using ICities;
using KianCommons;
using PathController.Manager;
using System;

public class SerializableDataExtension : SerializableDataExtensionBase {
    const string DATA_ID = "PathController";
    public override void OnLoadData() {
        try {
            Log.Called();
            byte[] data = serializableDataManager.LoadData(DATA_ID);
            PathControllerManager.Deserialize(data);
            Log.Succeeded();
        } catch (Exception ex) { ex.Log(); }
    }
    public override void OnSaveData() {
        try {
            Log.Called();
            var data = PathControllerManager.Instance.Serialize();
            if (data != null) {
                serializableDataManager.SaveData(DATA_ID, data);
            }
            Log.Succeeded();
        } catch (Exception ex) { ex.Log(); }
    }
}
