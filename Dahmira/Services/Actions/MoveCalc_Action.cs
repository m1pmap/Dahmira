using Dahmira.Interfaces;
using Dahmira.Models;
using Dahmira_Log.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dahmira.Services.Actions
{
    public class MoveCalc_Action : IAction
    {
        MainWindow Window;
        List<(CalcProduct, int)> MovedItems;
        bool IsMoveUp;

        public MoveCalc_Action(MainWindow _window, List<(CalcProduct, int)> _movedItems, bool _isMoveUp) 
        {
            Window = _window;
            MovedItems = _movedItems;
            IsMoveUp = _isMoveUp;
        }
        public void Undo()
        {
            try
            {
                if (IsMoveUp)
                    MovedItems = MovedItems.OrderByDescending(x => x.Item2).ToList();
                else
                    MovedItems = MovedItems.OrderBy(x => x.Item2).ToList();

                foreach (var movedItem in MovedItems)
                {
                    Window.calcItems.Remove(movedItem.Item1);
                    Window.calcItems.Insert(movedItem.Item2, movedItem.Item1);
                }

                if (!Window.isCalcOpened)
                {
                    Window.priceCalcButton_Click(Window, new RoutedEventArgs());
                }

                if (IsMoveUp)
                    Window.CalcDataGrid.SelectedItem = MovedItems[MovedItems.Count - 1].Item1;
                else
                    Window.CalcDataGrid.SelectedItem = MovedItems[0].Item1;

                ICalcController calcController = new CalcController_Services();
                calcController.Refresh(Window.CalcDataGrid, Window.calcItems);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }
    }
}
