using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    public abstract class BudgetItem : INotifyPropertyChanged
    {
        private decimal doNotUseAmount;
        private BudgetBucket doNotUseBucket;
        public event PropertyChangedEventHandler PropertyChanged;

        public decimal Amount
        {
            get { return this.doNotUseAmount; }

            set
            {
                if (value == this.doNotUseAmount)
                {
                    return;
                }
                this.doNotUseAmount = value;
                OnPropertyChanged();
            }
        }

        public BudgetBucket Bucket
        {
            get { return this.doNotUseBucket; }
            set
            {
                if (this.doNotUseBucket != null)
                {
                    this.doNotUseBucket.PropertyChanged -= OnBucketPropertyChanged;
                }

                this.doNotUseBucket = value;
                if (this.doNotUseBucket != null)
                {
                    this.doNotUseBucket.PropertyChanged += OnBucketPropertyChanged;
                }

                OnPropertyChanged();
            }
        }

        public string Summary
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    "{0} {1}: {2}",
                    Bucket.TypeDescription.AnOrA(true),
                    EnsureNoRepeatedLastWord(Bucket.TypeDescription, GetType().Name),
                    Bucket.Description);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as BudgetItem;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Bucket != null ? Bucket.GetHashCode() * GetType().GetHashCode() : 0);
            }
        }

        protected bool Equals([NotNull] BudgetItem other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return Equals(Bucket, other.Bucket) && GetType() == other.GetType();
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnBucketPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // This is to trigger updates dependent on Bucket, but isn't updated when Active is toggled. For example binding to bucket
            // and using the bucket to colour converter. This converter must return grey when the bucket is inactive.
            if (e.PropertyName == "Active")
            {
                OnPropertyChanged("Bucket");
            }
        }

        private static string EnsureNoRepeatedLastWord(string sentence1, string sentence2)
        {
            if (string.IsNullOrWhiteSpace(sentence1) || string.IsNullOrWhiteSpace(sentence2))
            {
                return string.Empty;
            }

            sentence1 = sentence1.Trim();
            sentence2 = sentence2.Trim();

            string lastWord;
            int wordIndex = sentence1.LastIndexOf(' ');
            if (wordIndex <= 0)
            {
                lastWord = sentence1;
            }
            else
            {
                lastWord = sentence1.Substring(wordIndex + 1);
            }

            string firstWord;
            wordIndex = sentence2.IndexOf(' ');
            if (wordIndex <= 0)
            {
                firstWord = sentence2;
                wordIndex = firstWord.Length;
            }
            else
            {
                firstWord = sentence2.Substring(0, wordIndex);
            }

            if (lastWord == firstWord)
            {
                return string.Format(CultureInfo.CurrentCulture, "{0}{1}", sentence1, sentence2.Substring(wordIndex));
            }

            return string.Format(CultureInfo.CurrentCulture, "{0} {1}", sentence1, sentence2);
        }

        public static bool operator ==(BudgetItem left, BudgetItem right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BudgetItem left, BudgetItem right)
        {
            return !Equals(left, right);
        }
    }
}