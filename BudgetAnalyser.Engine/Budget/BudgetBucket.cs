using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace BudgetAnalyser.Engine.Budget
{
    [DebuggerDisplay("BudgetBucket {Code} {Description}")]
    [XmlInclude(typeof(IncomeBudgetBucket))]
    [XmlInclude(typeof(SpentMonthlyExpense))]
    [XmlInclude(typeof(SavedUpForExpense))]
    public abstract class BudgetBucket : IModelValidate
    {
        private string doNotUseCode;
        protected BudgetBucket() { }
        protected BudgetBucket(string code, string name)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            Description = name;
            Code = code;
        }

        public Guid? Id { get; set; }
        public string Code
        {
            get { return this.doNotUseCode; }

            set { this.doNotUseCode = value.ToUpperInvariant(); }
        }

        public string Description { get; set; }

        public virtual string TypeDescription
        {
            get { return GetType().Name; }
        }

        public static bool operator ==(BudgetBucket obj1, BudgetBucket obj2)
        {
            object obj3 = obj1, obj4 = obj2;
            if (obj3 == null && obj4 == null)
            {
                return true;
            }

            if (obj3 == null || obj4 == null)
            {
                return false;
            }

            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(BudgetBucket obj1, BudgetBucket obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            var otherBucket = obj as BudgetBucket;
            if (otherBucket == null)
            {
                return false;
            }

            return Code == otherBucket.Code;
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} {1}", Code, Description);
        }

        public bool Validate(StringBuilder validationMessages)
        {
            bool retval = true;
            if (string.IsNullOrWhiteSpace(Code))
            {
                validationMessages.AppendFormat("Budget Bucket {0} is invalid, Code must be a small textual code.", Code);
                retval = false;
            }
            else
            {
                if (Code.Length > 7)
                {
                    validationMessages.AppendFormat("Budget Bucket {0} - {1} is invalid, Code must be a small textual code less than 7 characters.", Code, Description);
                    retval = false;
                }
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                validationMessages.AppendFormat("Budget Bucket {0} is invalid, Description must not be blank.", Code);
                retval = false;
            }

            return retval;
        }
    }
}