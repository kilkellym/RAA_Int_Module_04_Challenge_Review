#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
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
    public class Command3 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // 0. set variables
            int counter = 0;

            // 1. get all spaces
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_MEPSpaces);

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Insert groups to spaces");

                // 2. loop through spaces
                foreach (Space curSpace in collector)
                {
                    // 2b. get space group
                    string groupName = curSpace.LookupParameter("Comments").AsString();

                    if (string.IsNullOrEmpty(groupName))
                        continue;

                    GroupType curGroup = GetGroupTypeByName(doc, groupName);

                    // 2c. get space point
                    LocationPoint spaceLoc = curSpace.Location as LocationPoint;
                    XYZ spacePoint = spaceLoc.Point;

                    // 2d. insert group
                    doc.Create.PlaceGroup(spacePoint, curGroup);
                    counter++;
                }
                t.Commit();
            }
            TaskDialog.Show("Complete", $"Inserted {counter} groups into the current model");

            return Result.Succeeded;
        }

        private GroupType GetGroupTypeByName(Document doc, string groupName)
        {
            GroupType returnGroup = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                .WhereElementIsElementType()
                .Where(r => r.Name == groupName)
                .Cast<GroupType>().First();
            return returnGroup;
        }

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand2";
            string buttonTitle = "Button 2";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 2");

            return myButtonData1.Data;
        }
    }
}
