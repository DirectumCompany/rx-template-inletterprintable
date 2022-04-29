using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace GD.PrintableTemplate.Structures.Module
{

  /// <summary>
  /// Результат создания печатной формы.
  /// </summary>
  [Public]
  partial class GeneratePrintableFormResult
  {
    public bool IsSuccess {get; set;}
    
    public List<string> Errors {get; set;}
  }
  
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