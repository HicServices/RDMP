using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReusableUIComponents
{
    /// <summary>
    /// Filters a Listbox or ComboBox
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FilterControl<T> where T:Control
    {
        private Dictionary<T,List<object>> HiddenNodesDictionary = new Dictionary<T, List<object>>();
        
        /// <summary>
        /// Filters the listbox according to the text filter, note elements removed from the listbox are maintained by this class as a 
        /// HiddenNodes list meaning that the items will be re-added if the filter is adjusted to be less restrictive.  
        /// 
        /// IMPORTANT: if you modify the Items collection (e.g. clearing and repopulating it) you should call ClearHiddenNodes otherwise when the user next
        /// adjusts the filter your hidden nodes will creep back into the listbox on the sly
        /// 
        /// ALSO IMPORTANT: the filter is applied based on ToString and is case INSENSITIVE
        /// </summary>
        /// <param name="control"></param>
        /// <param name="filter"></param>
        public void ApplyFilter(T control, string filter)
        {
            //never seen this listbox before add it to the persistence dictionary
            if(!HiddenNodesDictionary.ContainsKey(control))
                HiddenNodesDictionary.Add(control,new List<object>());//add it

            //case insensitive the filter
            filter = filter.ToLower();

            ListBox listbox;
            ComboBox comboBox;
            
            listbox = control as ListBox;
            comboBox = control as ComboBox;

            if(listbox == null && comboBox == null)
                throw new NotSupportedException("Expected control to either be a listbox or a combo box but it was " + control + " (Type="+typeof(T).Name+")");
            
            //get all the currently visible listbox items
            List<object> allOjbects = new List<object>();


            //and all the ones that are currently hidden
            allOjbects.AddRange(HiddenNodesDictionary[control]);

            if (listbox != null)
            {
                allOjbects.AddRange(listbox.Items.Cast<object>());
                listbox.Items.Clear();
            }
            else
            {
                allOjbects.AddRange(comboBox.Items.Cast<object>()); 
                comboBox.Items.Clear();
            }
            
            //clear the hidden list
            HiddenNodesDictionary[control].Clear();

            //decide which destination (hidden or listbox) depending on the ToString method of your object
            foreach (object o in allOjbects)
            {
                if (o.ToString().ToLower().Contains(filter))
                {
                    if (listbox != null)
                        listbox.Items.Add(o);
                    else
                        comboBox.Items.Add(o);
                }
                else
                    HiddenNodesDictionary[control].Add(o);
            }
        }

        /// <summary>
        /// Call this method when you are repopulating your listbox yourself and don't want any objects hanging around in the HiddenNodes collection waiting 
        /// to be re-added once the user removes/edits the filter
        /// </summary>
        /// <param name="listbox"></param>
        public void ClearHiddenNodes(T listbox)
        {
            //never seen this listbox before
            if (!HiddenNodesDictionary.ContainsKey(listbox))
                HiddenNodesDictionary.Add(listbox, new List<object>());//add it

            HiddenNodesDictionary[listbox].Clear();

        }
    }
}
