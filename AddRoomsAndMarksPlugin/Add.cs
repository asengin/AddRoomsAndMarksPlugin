using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddRoomsAndMarksPlugin
{
    [Transaction(TransactionMode.Manual)]
    public class Add : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            List<Level> levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .OfType<Level>()
                .ToList();

            if (levels == null)
            {
                TaskDialog.Show("Ошибка", "Нет этажей для помещений");
                return Result.Cancelled;
            }

            using (Transaction ts = new Transaction(doc, "Расстановка помещений"))
            {
                ts.Start();

                List<ElementId> roomsID = new List<ElementId>();
                foreach (Level level in levels)
                    roomsID.AddRange(doc.Create.NewRooms2(level));  //Создаем помещения и получаем список всех ElementId этих помещений

                foreach (ElementId roomId in roomsID)
                {
                    Room roomById = (doc.GetElement(roomId) as Room);
                    string nameLevel = roomById.Level.Name.Substring(8); //Вытаскиваем № уровня, обрезаем лишние символы
                    //string nameNumberRoom = roomById.Name.Substring(10); //Аналогично № помещения
                    string nameNumberRoom = roomById.Number;
                    roomById.Number = string.Empty; roomById.Name = string.Empty; //Очищаем поле Номер и Имя
                    //roomById.Name = $"{nameLevel}_{nameNumberRoom}"; //Присваиваем данному помещению сформированное имя в Name
                    roomById.Number = $"{nameLevel}_{nameNumberRoom}"; //Присваиваем данному помещению сформированное имя в Number, чтобы был ов рамочке
                    doc.Create.NewRoomTag(new LinkElementId(roomId), new UV(0, 0), null);
                }

                ts.Commit();
            }
            return Result.Succeeded;
        }
    }
}
