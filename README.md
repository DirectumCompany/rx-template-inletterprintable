# rx-template-inletterprintable
Репозиторий с шаблоном разработки "Печатная форма входящего письма".

## Описание 
Шаблон позволяет реализовать на заказном слое формирование печатной формы входящего письма со штампами электронной подписи и регистрационных данных в формате PDF.  
В печатную форму проставляются штамп регистрационных данных входящего документа, штамп регистрационных данных исходящего документа из поля "В ответ на", штамп электронной подписи.  
Координаты и размеры штампов запрашиваются через диалоговое окно.  
Печатная форма сохраняется в новую версию с примечанием "Версия для печати" либо в последнюю существующую версию с таким примечанием.

Состав объектов разработки:
* Клиентская функция GeneratePrintableForm.

## Варианты расширения функциональности на проектах
1.	Перекрыть документ "Входящее письмо" (имя IncommingLetter).
2.  Добавить новое действие. В событии "Выполнение" вызвать функцию  
``` 
if (_obj.Versions.Where(x => Signatures.Get(x).Where(s => s.SignatureType == SignatureType.Approval).Any()).Any() == false)
  e.AddWarning(PrintableTemplate.Resources.NoExistsSignedVersion);
else
{
  if (PrintableTemplate.PublicFunctions.Module.GeneratePrintableForm(_obj))
    e.AddInformation(PrintableTemplate.Resources.ActionResult);
}
```
3.  В событии "Возможность выполнения" задать условие доступности действия
``` 
return !_obj.State.IsInserted && _obj.HasVersions && !_obj.State.IsChanged && _obj.AccessRights.CanUpdate() && !Locks.GetLockInfo(_obj).IsLockedByOther;
```

## Порядок установки
Для работы требуется установленный Directum RX и решение Интеграция с МЭДО версии 4.0.  
А так же неоходимо обновить сторонние библиотеки Sungero.AsposeExtensions.dll и MEDOSerializingXML.dll в модуле "PrintableTemplate". Необходимо взять эти библиотеки из модуля "MEDO" решения Интеграция с МЭДО.

### Установка для ознакомления
1. Склонировать репозиторий IncommingLetterPrintable в папку.
2. Указать в _ConfigSettings.xml DDS:
```xml
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" />
  <repository folderName="RX" solutionType="Base" url="<адрес локального репозитория>" />
  <repository folderName="<Папка из п.1>" solutionType="Work" 
     url="https://customdevtfs.directum.ru/tfs/GovernmentDepartmentsRX/GovernmentStSol/_git/IncommingLetterPrintable" />
</block>
```

### Установка для использования на проекте
Возможные варианты:

**A. Fork репозитория**
1. Сделать fork репозитория IncommingLetterPrintable для своей учетной записи.
2. Склонировать созданный в п. 1 репозиторий в папку.
3. Указать в _ConfigSettings.xml DDS:
``` xml
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" /> 
  <repository folderName="<Папка из п.2>" solutionType="Work" 
     url="<Адрес репозитория gitHub учетной записи пользователя из п. 1>" />
</block>
```

**B. Подключение на базовый слой.**

Вариант не рекомендуется, так как при выходе версии шаблона разработки не гарантируется обратная совместимость.
1. Склонировать репозиторий IncommingLetterPrintable в папку.
2. Указать в _ConfigSettings.xml DDS:
``` xml
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" /> 
  <repository folderName="<Папка из п.1>" solutionType="Base" 
     url="<Адрес репозитория gitHub>" />
  <repository folderName="<Папка для рабочего слоя>" solutionType="Work" 
     url="https://customdevtfs.directum.ru/tfs/GovernmentDepartmentsRX/GovernmentStSol/_git/IncommingLetterPrintable" />
</block>
```

**C. Копирование репозитория в систему контроля версий.**

Рекомендуемый вариант для проектов внедрения.
1. В системе контроля версий с поддержкой git создать новый репозиторий.
2. Склонировать репозиторий IncommingLetterPrintable в папку с ключом `--mirror`.
3. Перейти в папку из п. 2.
4. Импортировать клонированный репозиторий в систему контроля версий командой:

`git push –mirror <Адрес репозитория из п. 1>`
