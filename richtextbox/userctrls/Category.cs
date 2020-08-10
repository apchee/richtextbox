using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace richtextbox.userctrls {
    public class Category : ViewModelBase {


        public int Id { get; set; }
        public string Title { get; set; } 
        public string Version { get; set; }
        public byte[] Content { get; set; }


        private ObservableCollection<Category> subCategories = new ObservableCollection<Category>();
        public ObservableCollection<Category> SubCategories { get => subCategories;
            set { subCategories = value; RaisePropertyChanged(); }
        }
    }
}
