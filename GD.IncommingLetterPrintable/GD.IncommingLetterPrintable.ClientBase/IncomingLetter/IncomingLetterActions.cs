using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.IncommingLetterPrintable.IncomingLetter;

namespace GD.IncommingLetterPrintable.Client
{
  partial class IncomingLetterActions
  {
    public virtual void CreatePrintableVersionGD(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.Versions.Where(x => Signatures.Get(x).Where(s => s.SignatureType == SignatureType.Approval).Any()).Any() == false)
        e.AddWarning(PrintableTemplate.Resources.NoExistsSignedVersion);
      else
      {
        PrintableTemplate.PublicFunctions.Module.GeneratePrintableForm(_obj);
        e.AddInformation(PrintableTemplate.Resources.ActionResult);    
      }
    }

    public virtual bool CanCreatePrintableVersionGD(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted && _obj.HasVersions && !_obj.State.IsChanged &&
        _obj.AccessRights.CanUpdate() && !Locks.GetLockInfo(_obj).IsLockedByOther;
    }

  }

}