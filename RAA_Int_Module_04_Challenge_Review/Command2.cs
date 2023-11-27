#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Forms = System.Windows.Forms;

#endregion

namespace RAA_Int_Module_04_Challenge_Review
{
    [Transaction(TransactionMode.Manual)]
    public class Command2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // 1. prompt user to select RVT file
            // NOTE: be sure to add reference to System.Windows.Forms
            string revitFile = string.Empty;

            Forms.OpenFileDialog ofd = new Forms.OpenFileDialog();
            ofd.Title = "Select Revit file";
            ofd.InitialDirectory = @"C:\";
            ofd.Filter = "Revit files (*.rvt)|*.rvt";
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() != Forms.DialogResult.OK)
                return Result.Failed;

            revitFile = ofd.FileName;

            // 2. Open selected file in background
            UIDocument otherUIDoc = uiapp.OpenAndActivateDocument(revitFile);
            Document otherDoc = otherUIDoc.Document;

            // 3. get list of groups from other doc
            List<ElementId> groupList = new FilteredElementCollector(otherDoc)
                    .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                    .Select(item => item.Id).ToList();

            // 4. copy groups into current file
            CopyPasteOptions options = new CopyPasteOptions();

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Copy groups");
                List<ElementId> newGroupList = ElementTransformUtils.
                    CopyElements(otherDoc, groupList, doc, null, options).ToList();
                t.Commit();
            }

            // 5. make original doc active then close other doc
            uiapp.OpenAndActivateDocument(doc.PathName);
            otherDoc.Close(false);

            TaskDialog.Show("Complete", $"Copied {groupList.Count} groups into the current model.");

            return Result.Succeeded;
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
