using GD.GovernmentSolution;
using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace GD.PrintableTemplate.Client
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Создание версии входящего письма с штампами рег. данных и ЭП.
    /// </summary>
    /// <param name="letter">Входящий документ.</param>
    /// <returns>Было ли запущено формирование версии.</returns>
    [Public]
    public bool GeneratePrintableForm(Sungero.RecordManagement.IIncomingLetter letter)
    {      
      if (Sungero.Company.Employees.Current == null)
        return false;
      
      var dialog = Dialogs.CreateInputDialog("");
      var personalSettings = GovernmentSolution.PersonalSettings.As(Sungero.Docflow.PublicFunctions.PersonalSetting.GetPersonalSettings(Sungero.Company.Employees.Current));
      
      var stampCoordinates = personalSettings.StampCoordinates.FirstOrDefault(s => string.Equals(s.Type.Value.Value, GovernmentSolution.OutgoingLetterStampCoordinates.Type.Reg.Value));
      var isStamp = stampCoordinates != null;
      var pageRegData = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Reg),
                                                        GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.PageNumber.LocalizedName), true, isStamp ? stampCoordinates.PageNumber : null);
      var horizontallyRegData = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Reg),
                                                                GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.Horizontally.LocalizedName), true, isStamp ? stampCoordinates.Horizontally : null);
      var verticallyRegData = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Reg),
                                                              GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.Vertically.LocalizedName), true, isStamp ? stampCoordinates.Vertically : null);
      var heightRegData = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Reg),
                                                          GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.Height.LocalizedName), true, isStamp ? stampCoordinates.Height : null);
      var widthRegData = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Reg),
                                                         GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.Width.LocalizedName), true, isStamp ? stampCoordinates.Width : null);
      
      stampCoordinates = personalSettings.StampCoordinates.FirstOrDefault(s => string.Equals(s.Type.Value.Value, GovernmentSolution.OutgoingLetterStampCoordinates.Type.Sig.Value));
      isStamp = stampCoordinates != null;
      var pageSignature = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Sig),
                                                          GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.PageNumber.LocalizedName), true, isStamp ? stampCoordinates.PageNumber : null);
      var horizontallySignature = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Sig),
                                                                  GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.Horizontally.LocalizedName), true, isStamp ? stampCoordinates.Horizontally : null);
      var verticallySignature = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Sig),
                                                                GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.Vertically.LocalizedName), true, isStamp ? stampCoordinates.Vertically : null);
      var heightSignature = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Sig),
                                                            GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.Height.LocalizedName), true, isStamp ? stampCoordinates.Height : null);
      var widthSignature = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Sig),
                                                           GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.Width.LocalizedName), true, isStamp ? stampCoordinates.Width : null);
      
      if (dialog.Show() == DialogButtons.Ok)
      {        
        var stampRegCoordin = Structures.Module.StampCoordinates.Create(pageRegData.Value.Value,
                                                                        horizontallyRegData.Value.Value,
                                                                        verticallyRegData.Value.Value,
                                                                        heightRegData.Value.Value,
                                                                        widthRegData.Value.Value);
        
        var stampRegInResponseToCoordin = Structures.Module.StampCoordinates.Create(pageRegData.Value.Value,
                                                                                    horizontallyRegData.Value.Value,
                                                                                    verticallyRegData.Value.Value + heightRegData.Value.Value,
                                                                                    heightRegData.Value.Value,
                                                                                    widthRegData.Value.Value);
        
        var stampSignatureCoordin = Structures.Module.StampCoordinates.Create(pageSignature.Value.Value,
                                                                              horizontallySignature.Value.Value,
                                                                              verticallySignature.Value.Value,
                                                                              heightSignature.Value.Value,
                                                                              widthSignature.Value.Value);
        
        Functions.Module.Remote.GenerateNewVersionWithStamp(letter, stampRegCoordin, stampRegInResponseToCoordin, stampSignatureCoordin);
        return true;
      } 
      else
      {
        return false;
      }
    }
  }
}
