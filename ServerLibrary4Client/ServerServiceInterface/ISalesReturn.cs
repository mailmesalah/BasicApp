using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace ServerServiceInterface
{    
    [ServiceContract]
    public interface ISalesReturn
    {
        [OperationContract]
        bool CreateBill(CSalesReturn oSalesReturn);
        [OperationContract]
        CSalesReturn ReadBill(string billNo,string financialCode);
        [OperationContract]
        CSalesReturn ReadSalesBill(string billNo, string financialCode);
        [OperationContract]
        bool UpdateBill(CSalesReturn oSalesReturn);
        [OperationContract]
        bool DeleteBill(string billNo,string financialCode);        
        [OperationContract]
        int ReadNextBillNo(string financialCode);
     
    }


    
    [DataContract]
    public class CSalesReturn
    {
        int id;
        string billNo;
        DateTime billDateTime = new DateTime();
        string refBillNo;
        DateTime refBillDateTime = new DateTime();
        string customer;
        string customerCode;
        string customerAddress;
        string narration;
        decimal advance;
        decimal expense;
        decimal discount;
        string financialCode;
        List<CSalesReturnDetails> details= new List<CSalesReturnDetails>();
 
        [DataMember]
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        [DataMember]
        public string BillNo
        {
            get { return billNo; }
            set { billNo = value; }
        }
        
        [DataMember]
        public DateTime BillDateTime
        {
            get { return billDateTime; }
            set { billDateTime = value; }
        }

        [DataMember]
        public string RefBillNo
        {
            get { return refBillNo; }
            set { refBillNo = value; }
        }

        [DataMember]
        public DateTime RefBillDateTime
        {
            get { return refBillDateTime; }
            set { refBillDateTime = value; }
        }

        [DataMember]
        public string Customer
        {
            get { return customer; }
            set { customer = value; }
        }

        [DataMember]
        public string CustomerCode
        {
            get { return customerCode; }
            set { customerCode = value; }
        }

        [DataMember]
        public string CustomerAddress
        {
            get { return customerAddress; }
            set { customerAddress = value; }
        }

        [DataMember]
        public string Narration
        {
            get { return narration; }
            set { narration = value; }
        }

        [DataMember]
        public decimal Advance
        {
            get { return advance; }
            set { advance = value; }
        }

        [DataMember]
        public decimal Expense
        {
            get { return expense; }
            set { expense = value; }
        }

        [DataMember]
        public decimal Discount
        {
            get { return discount; }
            set { discount = value; }
        }

        [DataMember]
        public string FinancialCode
        {
            get { return financialCode; }
            set { financialCode = value; }
        }

        [DataMember]
        public List<CSalesReturnDetails> Details
        {
            get { return details; }
            set { details = value; }
        }
    }


    [DataContract]
    public class CSalesReturnDetails
    {
        int serialNo;
        string productCode;
        string product;
        string salesUnit;
        string salesUnitCode;
        decimal salesUnitValue;
        decimal quantity;
        decimal salesRate;
        decimal mrp;
        string barcode;
        decimal total;
        decimal oldQuantity;

        [DataMember]
        public int SerialNo
        {
            get { return serialNo; }
            set { serialNo = value; }
        }

        [DataMember]
        public string ProductCode
        {
            get { return productCode; }
            set { productCode = value; }
        }

        [DataMember]
        public string Product
        {
            get { return product; }
            set { product = value; }
        }

        [DataMember]
        public string SalesReturnUnit
        {
            get { return salesUnit; }
            set { salesUnit = value; }
        }

        [DataMember]
        public string SalesReturnUnitCode
        {
            get { return salesUnitCode; }
            set { salesUnitCode = value; }
        }

        [DataMember]
        public decimal SalesReturnUnitValue
        {
            get { return salesUnitValue; }
            set { salesUnitValue = value; }
        }

        [DataMember]
        public decimal Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        [DataMember]
        public decimal SalesReturnRate
        {
            get { return salesRate; }
            set { salesRate = value; }
        }

        [DataMember]
        public decimal MRP
        {
            get { return mrp; }
            set { mrp = value; }
        }

        [DataMember]
        public string Barcode
        {
            get { return barcode; }
            set { barcode = value; }
        }

        [DataMember]
        public decimal Total
        {
            get { return total; }
            set { total = value; }
        }

        [DataMember]
        public decimal OldQuantity
        {
            get { return oldQuantity; }
            set { oldQuantity = value; }
        }
    }
}
