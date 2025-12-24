# Печатная форма входящего письма
Репозиторий с шаблоном разработки "Печатная форма входящего письма".

## Описание 
Шаблон позволяет реализовать на заказном слое формирование печатной формы входящего письма со штампами электронной подписи и регистрационных данных в формате PDF.  
В печатную форму проставляются штамп регистрационных данных входящего документа, штамп регистрационных данных исходящего документа из поля "В ответ на", штамп электронной подписи.  
Координаты и размеры штампов запрашиваются через диалоговое окно.  
Печатная форма сохраняется в новую версию с примечанием "Версия для печати" либо в последнюю существующую версию с таким примечанием.

Состав объектов разработки:
* Клиентская функция GeneratePrintableForm.
> [!NOTE]
> Замечания и пожеланию по развитию шаблона разработки фиксируйте через [Issues](https://github.com/DirectumCompany/rx-template-counterpartiescleaning/issues).
При оформлении ошибки, опишите сценарий для воспроизведения. Для пожеланий приведите обоснование для описываемых изменений - частоту использования, бизнес-ценность, риски и/или эффект от реализации.
> 
> Внимание! Изменения будут вноситься только в новые версии.

## Варианты расширения функциональности на проектах
1.	Перекрыть документ "Входящее письмо" (имя IncommingLetter).
2.  Добавить новое действие. В событии "Выполнение" вызвать функцию  
``` 
if (_obj.Versions.Where(x => Signatures.Get(x).Where(s => s.SignatureType == SignatureType.Approval).Any()).Any() == false)
  e.AddWarning(PrintableTemplate.Resources.NoExistsSignedVersion);
else
{
  var result = PrintableTemplate.PublicFunctions.Module.GeneratePrintableForm(_obj);
  if (result.IsSuccess)
    e.AddInformation(PrintableTemplate.Resources.ActionResult);
  else
    result.Errors.ForEach(e.AddWarning);
}
```
3.  В событии "Возможность выполнения" задать условие доступности действия
``` 
return !_obj.State.IsInserted && _obj.HasVersions && !_obj.State.IsChanged && _obj.AccessRights.CanUpdate() && !Locks.GetLockInfo(_obj).IsLockedByOther;
```

## Порядок установки
Для работы требуется установленный Directum RX и решение Интеграция с МЭДО версии 4.7.  
А так же неоходимо обновить стороннюю библиотеку MEDOSerializingXML в модуле "PrintableTemplate". Необходимо взять эту библиотеку из модуля "MEDO" решения Интеграция с МЭДО.

### Установка для ознакомления
1. Склонировать репозиторий с IncommingLetterPrintable в папку.
2. Указать в config.yml в разделе DevelopmentStudio:
```xml
   GIT_ROOT_DIRECTORY: '<Папка из п.1>'
   REPOSITORIES:
      repository:
      -   '@folderName': 'work'
          '@solutionType': 'Work'
          '@url': https://github.com/DirectumCompany/rx-template-inletterprintable.git'
      -   '@folderName': 'base'
          '@solutionType': 'Base'
          '@url': ''
```

### Установка для использования на проекте
Возможные варианты:

**A. Fork репозитория**
1. Сделать fork репозитория IncommingLetterPrintable для своей учетной записи.
2. Склонировать созданный в п. 1 репозиторий в папку.
3. Указать в config.yml в разделе DevelopmentStudio:
```xml
   GIT_ROOT_DIRECTORY: '<Папка из п.2>'
   REPOSITORIES:
      repository:
      -   '@folderName': 'work'
          '@solutionType': 'Work'
          '@url': https://github.com/DirectumCompany/rx-template-inletterprintable.git'
      -   '@folderName': 'base'
          '@solutionType': 'Base'
          '@url': ''
```

**B. Подключение на базовый слой.**

Вариант не рекомендуется, так как при выходе версии шаблона разработки не гарантируется обратная совместимость.
1. Склонировать репозиторий IncommingLetterPrintable в папку.
2. Указать в config.yml в разделе DevelopmentStudio:
```xml
   GIT_ROOT_DIRECTORY: '<Папка из п.1>'
   REPOSITORIES:
      repository:
      -   '@folderName': 'work'
          '@solutionType': 'Work'
          '@url': '<Адрес репозитория для рабочего слоя>'
      -   '@folderName': 'base'
          '@solutionType': 'Base'
          '@url': ''
      -   '@folderName': 'base'
          '@solutionType': 'Base'
          '@url': 'https://github.com/DirectumCompany/rx-template-inletterprintable.git'
```

**C. Копирование репозитория в систему контроля версий.**

Рекомендуемый вариант для проектов внедрения.
1. В системе контроля версий с поддержкой git создать новый репозиторий.
2. Склонировать репозиторий IncommingLetterPrintable в папку с ключом `--mirror`.
3. Перейти в папку из п. 2.
4. Импортировать клонированный репозиторий в систему контроля версий командой:
`git push –mirror <Адрес репозитория из п. 1>`




