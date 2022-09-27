using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace ServerServiceInterface
{    
    [ServiceContract]
    public interface IPurchase
    {
        [OperationContract]
        bool CreateBill(CPurchase oPurchase);
        [OperationContract]
        CPurchase ReadBill(string billNo,string financialCode);
        [OperationContract]
        bool UpdateBill(CPurchase oPurchase);
        [OperationContract]
        bool DeleteBill(string billNo,string financialCode);        
        [OperationContract]
        int ReadNextBillNo(string financialCode);
     
    }


    
    [DataContract]
    public class CPurchase
    {
        int id;
        string billNo;
        DateTime billDateTime = new DateTime();
        string supplier;
        string supplierCode;
        string supplierAddress;
        string narration;
        decimal advance;
        decimal expense;
        decimal discount;
        string financialCode;
        List<CPurchaseDetails> details= new List<CPurchaseDetails>();
 
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
        public string Supplier
        {
            get { return supplier; }
            set { supplier = value; }
        }

        [DataMember]
        public string SupplierCode
        {
            get { return supplierCode; }
            set { supplierCode = value; }
        }

        [DataMember]
        public string SupplierAddress
        {
            get { return supplierAddress; }
            set { supplierAddress = value; }
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
        public List<CPurchaseDetails> Details
        {
            get { return details; }
            set { details = value; }
        }
    }


    [DataContract]
    public class CPurchaseDetails
    {
        int serialNo;
        string productCode;
        string product;
        string purchaseUnit;
        string purchaseUnitCode;
        decimal purchaseUnitValue;
        decimal quantity;
        decimal purchaseRate;
        decimal mrp;
        string barcode;
        decimal total;

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
        public string PurchaseUnit
        {
            get { return purchaseUnit; }
            set { purchaseUnit = value; }
        }

        [DataMember]
        public string PurchaseUnitCode
        {
            get { return purchaseUnitCode; }
            set { purchaseUnitCode = value; }
        }

        [DataMember]
        public decimal PurchaseUnitValue
        {
            get { return purchaseUnitValue; }
            set { purchaseUnitValue = value; }
        }

        [DataMember]
        public decimal Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        [DataMember]
        public decimal PurchaseRate
        {
            get { return purchaseRate; }
            set { purchaseRate = value; }
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
    }
}
