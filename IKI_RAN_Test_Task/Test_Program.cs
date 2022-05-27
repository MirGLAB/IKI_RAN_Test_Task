using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IKI_RAN_Test_Task
{
    internal class Test_Program
    {
        //
        // Метод для копирования файлов через перетаскивание из одного каталога в другой и перезаписывания
        // в случае совпадения имен файлов. В случае если операция невозможна возвращает true.
        //
        // DragDropOperationContext context - данные объектов перетаскивания
        // ScriptingObject destination - место назначение, куда происходит претаскивание объекта
        // DragDropEffects dropEffect - эффекты перетаскивания (перемещение, копирование и др.)
        public override bool dummy(DragDropOperationContext context, ScriptingObject destination, DragDropEffects dropEffect)
        {
            // если нет доступа к EntityIdentifier.InfoObject
            if (!context.IsAllowed(EntityIdentifier.InfoObject))
                return true;
            
            // если перетаскивание не происходит с копированием завершение
            if (dropEffect != DragDropEffects.Copy)
                return true;

            // если destination не может быть преобразован к DataContainer и InfoObject завершение
            var c = destination as DataContainer;
            var io = destination as InfoObject;
            if (c == null && io == null)
                return true;
            
            // если не удалось получить шаблоны завершение
            var folderTemplate = Service.GetTemplate(@"Containers\ESPD\Distributives");
            var targetTemplate = Service.GetTemplate(@"InfoObjects\ESPD\DistributionPackage");
            if (folderTemplate == null || targetTemplate == null)
                return true;
            
            // если destination преобразован к DataContainer и его tmplate == folderTemplate
            if (c != null && folderTemplate == c.Template)
            {
                if (context.RawData.GetDataPresent(DataFormats.FileDrop))
                {
                    context.Handled = true;
                    // получение файлов для претаскивания из context
                    var files = context.RawData.GetData<string[]>(DataFormats.FileDrop).Where(File.Exists);
                    // словарь с именами существующих файлов и данными в них
                    var existingByName = new Dictionary<String, InfoObject>(StringComparer.CurrentCultureIgnoreCase);
                    c.RootInfoObjects.ForEach2(o => existingByName[o.GetValue<String>("FileName")] = o);
                    // перезаписывание существующих файлов
                    foreach (var f in files)
                    {
                        var name = Path.GetFileNameWithoutExtension(f);
                        var targetIO = existingByName.GetValueIfAny(name);
                        if (targetIO == null)
                            targetIO = new InfoObject(c, targetTemplate);
                        targetIO.GetAttribute("Body").Value.UploadFile(f);
                    }
                }
            }
            return true;
        }
    }
}
