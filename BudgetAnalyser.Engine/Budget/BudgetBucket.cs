using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BudgetAnalyser.Engine.Budget
{
    [DebuggerDisplay("BudgetBucket {Code} {Description}")]
    [XmlInclude(typeof (IncomeBudgetBucket))]
    [XmlInclude(typeof (SpentMonthlyExpense))]
    [XmlInclude(typeof (SavedUpForExpense))]
    public abstract class BudgetBucket : IModelValidate
    {
        private string doNotUseCode;

        protected BudgetBucket()
        {
            this.Id = Guid.NewGuid();
        }

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

            this.Description = name;
            this.Code = code.ToUpperInvariant();
            this.Id = Guid.NewGuid();
        }

        public string Code
        {
            get { return this.doNotUseCode; }

            set { this.doNotUseCode = value.ToUpperInvariant(); }
        }

        public string Description { get; set; }

        [XmlAttribute]
        public Guid Id { get; set; }

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
            return this.Code.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} {1}", Code, Description);
        }

        public bool Validate(StringBuilder validationMessages)
        {
            bool retval = true;
            if (string.IsNullOrWhiteSpace(this.Code))
            {
                validationMessages.AppendFormat("Budget Bucket {0} is invalid, Code must be a small textual code.", this.Code);
                retval = false;
            }

            if (this.Code.Length > 7)
            {
                validationMessages.AppendFormat("Budget Bucket {0} - {1} is invalid, Code must be a small textual code less than 7 characters.", this.Code, this.Description);
                retval = false;
            }

            if (string.IsNullOrWhiteSpace(this.Description))
            {
                validationMessages.AppendFormat("Budget Bucket {0} is invalid, Description must not be blank.", this.Code);
                retval = false;
            }

            return retval;
        }
    }
}