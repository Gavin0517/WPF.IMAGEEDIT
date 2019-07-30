using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFPhotoEditorTool.Models;

namespace WPFPhotoEditorTool.ViewModels
{
    public static class Common
    {
        public static ICollection<EditMenu> GetEditMenus()
        {
            var editMenus = new List<EditMenu>() { };
            editMenus.Add(new EditMenu { DrawEnum= DrawEnum.Pen, Name = "btnPen", ToolTip = "画笔", Source = "Resources/shuazi_checked.png", CheckedSourceUrl = "Resources/shuazi_checked.png", SourceUrl = "Resources/shuazi.png" });
            editMenus.Add(new EditMenu { DrawEnum= DrawEnum.Square,Name = "btnSquare", ToolTip = "矩形", Source = "Resources/kuang.png", CheckedSourceUrl = "Resources/kuang_checked.png", SourceUrl = "Resources/kuang.png" });
            editMenus.Add(new EditMenu { DrawEnum= DrawEnum.Arrow,Name = "btnArrow", ToolTip = "箭头", Source = "Resources/markforward.png", CheckedSourceUrl = "Resources/markforward_checked.png", SourceUrl = "Resources/markforward.png" });
            editMenus.Add(new EditMenu { DrawEnum= DrawEnum.None,Name = "btnSave", ToolTip = "保存图片", Source = "Resources/download.png" });
            editMenus.Add(new EditMenu { DrawEnum= DrawEnum.None, Name = "btnCancel", ToolTip = "撤销", Source = "Resources/callback.png" });
            editMenus.Add(new EditMenu { DrawEnum= DrawEnum.None, Name = "btnSendToCompared", ToolTip = "保存并发送至对比屏", Source = "Resources/sendToCompared.png" });
            editMenus.Add(new EditMenu { DrawEnum= DrawEnum.None, Name = "btnSend", ToolTip = "保存并发送聊天框", Source = "Resources/sendImage.png" });
            editMenus.Add(new EditMenu { DrawEnum = DrawEnum.None, Name = "btnClose", ToolTip = "退出关闭标记", Source = "Resources/cancel.png" });
            return editMenus;
        }
    }
}
