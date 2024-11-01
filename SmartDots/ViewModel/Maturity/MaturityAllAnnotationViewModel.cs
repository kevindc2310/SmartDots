﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid.LookUp;
using SmartDots.Helpers;
using SmartDots.Model;

namespace SmartDots.ViewModel
{
    public class MaturityAllAnnotationViewModel : MaturityBaseViewModel
    {
        private ObservableCollection<MaturityAnnotation> annotations;



        public ObservableCollection<MaturityAnnotation> Annotations
        {
            get { return annotations; }
            set
            {
                annotations = value;
                
                RaisePropertyChanged("Annotations");

                
            }
        }
    }
}
