using System;

namespace Localization.Net.Maintenance
{
    public abstract class BatchUpdatableTextSource : TextSourceBase
    {
        /// <summary>
        /// Supress OnTextsChanged events if this is greater than zero
        /// </summary>
        private int _updateScope = 0;
        private bool _hasChanges = false;
        public virtual IDisposable BeginUpdate()
        {
            ++_updateScope;
            return new UpdateToken(this);
        }

        public virtual void EndUpdate()
        {
            --_updateScope;
            if (_hasChanges)
            {
                OnTextsChanged();
            }
        }

        protected override void OnTextsChanged()
        {
            if (_updateScope == 0)
            {
                base.OnTextsChanged();
            }
            else
            {
                _hasChanges = true;
            }
        }

        protected class UpdateToken : IDisposable
        {
            private BatchUpdatableTextSource _owner;
            public UpdateToken(BatchUpdatableTextSource owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                _owner.EndUpdate();
            }
        }

    }
}
