using System.Collections.Generic;
using VectorEditor.Models;

namespace VectorEditor.Commands
{
    public class CreateCommand : IEditorCommand
    {
        private BaseShape shape;
        private IList<BaseShape> list;

        public CreateCommand(BaseShape shape, IList<BaseShape> list)
        {
            this.shape = shape;
            this.list = list;
        }

        public void Execute()
        {
            if (!list.Contains(shape))
                list.Add(shape);
        }

        public void Unexecute()
        {
            if (list.Contains(shape))
                list.Remove(shape);
        }
    }
}
