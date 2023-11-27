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

#endregion

namespace RAA_Int_Module_04_Challenge_Review
{
    [Transaction(TransactionMode.Manual)]
    public class Command4 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // 0. set variables
            int counter = 0;
            Document openDoc = null;

            // 1. Get open file with specific name
            foreach (Document curDoc in uiapp.Application.Documents)
            {
                if (curDoc.PathName.Contains("Sample 03"))
                {
                    openDoc = curDoc;
                }
            }

            if (openDoc == null)
                return Result.Failed;

            // 2. create element multi category filter to get walls and generic models
            List<BuiltInCategory> catList = new List<BuiltInCategory>()
                    {BuiltInCategory.OST_Walls, BuiltInCategory.OST_GenericModel };

            ElementMulticategoryFilter catFilter = new ElementMulticategoryFilter(catList);

            // 3. create filtered element collector to get elements
            FilteredElementCollector collector = new FilteredElementCollector(openDoc)
                    .WherePasses(catFilter)
                    .WhereElementIsNotElementType();

            // 4. get list of element Ids
            List<ElementId> elemList = collector.Select(elem => elem.Id).ToList();

            // 5. copy elements 
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Copy elements");
                ElementTransformUtils.CopyElements(openDoc, elemList, doc, null, new CopyPasteOptions());
                t.Commit();
            }

            TaskDialog.Show("Complete", $"Inserted {elemList.Count} elements into the current model.");

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
