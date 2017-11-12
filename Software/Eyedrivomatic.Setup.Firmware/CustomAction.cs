using System;
using ArduinoUploader;
using ArduinoUploader.Hardware;
using Eyedrivomatic.Hardware;
using WixToolset.Dtf.WindowsInstaller;

namespace Eyedrivomatic.Setup.Firmware
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult EnumerateDevices(Session session)
        {
            var devices = UsbSerialDeviceEnumerator.EnumerateDevices();
            var i = 1;
            foreach (var device in devices)
            {
                AddItemToDeviceList(session, i++, device.ConnectionString, device.FriendlyName);
            }

            return ActionResult.Success;
        }

        private static void AddItemToDeviceList(Session session, int index, string port, string name)
        {
            var db = session.Database;
            var sqlInsertTemp = db.Tables["ListBox"].SqlInsertString + " TEMPORARY";
            var view = db.OpenView(sqlInsertTemp);
            view.Execute(new Record("DEVICELISTBOXVALUES", index, port, name));
            view.Close();
        }

        [CustomAction]
        public static ActionResult UploadFirmware(Session session)
        {
            session.Log("Begin Firmware Upload");
            try
            {
                //var finder = new ElectronicHandEnumerationService();
                //var devices = finder.GetCandidateDevices();

                //if (!devices.Any())
                //{
                //    session.Log("ERROR: Device not found. Please connect your Eyedrivomatic \"Electronic Hand\" and install the appropriate drivers.");
                //    return ActionResult.Failure;
                //}


                //session.Log("Devices:");
                //foreach (var device in devices)
                //{
                //    session.Log($"\t{device.Port} {device.FriendlyName} - {device.Description} ");
                //}

                //if (devices.Count > 1)
                //{
                //    session.Log("ERROR: more than one Arduino is currently connected. To prevent accidental device malfunction, please only have your Eyedrivomatic \"Electronic Hand\" connected while updating the firware.");
                //    return ActionResult.Failure;
                //}

                var filepath = session.CustomActionData["FILEPATH"];

                var uploader = new ArduinoSketchUploader(
                    new ArduinoSketchUploaderOptions
                    {
                        FileName = filepath,
                        PortName = "COM4", //devices.First().Port,
                        ArduinoModel = ArduinoModel.UnoR3
                    });

                uploader.UploadSketch();
            }
            catch (Exception ex)
            {
                session.Log($"ERROR in custom action UploadFirmware {ex}.");
                return ActionResult.Failure;
            }

            session.Log("End Firmware Upload");
            return ActionResult.Success;
        }
    }
}
