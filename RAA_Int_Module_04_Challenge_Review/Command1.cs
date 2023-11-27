#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

#endregion

namespace RAA_Int_Module_04_Challenge_Review
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // 0. define variables
            int counter = 0;
            Document linkedDoc = null;
            RevitLinkInstance link = null;

            // 1. get all links
            FilteredElementCollector linkCollector = new FilteredElementCollector(doc)
                    .OfClass(typeof(RevitLinkType));

            // 2. loop through links and get doc if loaded
            foreach (RevitLinkType rvtLink in linkCollector)
            {
                if (rvtLink.GetLinkedFileStatus() == LinkedFileStatus.Loaded)
                {
                    link = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_RvtLinks)
                        .OfClass(typeof(RevitLinkInstance))
                        .Where(x => x.GetTypeId() == rvtLink.Id).First() as RevitLinkInstance;

                    linkedDoc = link.GetLinkDocument();
                }
            }

            // 3. get rooms from linked doc
            if (linkedDoc == null)
                return Result.Failed;

            List<Room> roomList = new FilteredElementCollector(linkedDoc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>().ToList();

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Copy rooms to spaces");

                // 4. get level of current view
                Level curLevel = doc.ActiveView.GenLevel;

                // 5. loop through rooms and create space based on room data
                foreach (Room curRoom in roomList)
                {
                    // 5b. get room data
                    string roomName = curRoom.Name;
                    string roomNum = curRoom.Number;
                    string roomComments = curRoom.LookupParameter("Comments").AsString();

                    // 5c. get room location point
                    LocationPoint roomPoint = curRoom.Location as LocationPoint;

                    // 5d. create space and transfer properties
                    SpatialElement newSpace = doc.Create.
                        NewSpace(curLevel, new UV(roomPoint.Point.X, roomPoint.Point.Y));

                    newSpace.Name = roomName;
                    newSpace.Number = roomNum;
                    newSpace.LookupParameter("Comments").Set(roomComments);

                    counter++;
                }

                // 6. turn off link in current view
                doc.ActiveView.HideElements(new List<ElementId> { link.Id });

                t.Commit();
            }

            TaskDialog.Show("Complete", $"Copied {counter} rooms to spaces.");

            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData1.Data;
        }
    }
}
