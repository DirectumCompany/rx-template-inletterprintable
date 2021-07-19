using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace GD.PrintableTemplate.Structures.Module
{
  /// <summary>
  /// Информация о типе штампа, его размерах и месте простановки.
  /// </summary>
  partial class StampCoordinates
  {    
    public int PageNumber {get; set;}
    
    public int Horizontally {get; set;}
    
    public int Vertically {get; set;}
    
    public int Height {get; set;}
    
    public int Width {get; set;}    
  }
}