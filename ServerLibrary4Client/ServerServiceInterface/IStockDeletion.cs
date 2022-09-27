using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace ServerServiceInterface
{    
    [ServiceContract]
    public interface IStockDeletion
    {
        [OperationContract]
        bool CreateBill(CStockDeletion oStockDeletion);
        [OperationContract]
        CStockDeletion ReadBill(string billNo,string financialCode);
        [OperationContract]
        bool UpdateBill(CStockDeletion oStockDeletion);
        [OperationContract]
        bool DeleteBill(string billNo,string financialCode);

        [OperationContract]
        int ReadNextBillNo(string financialCode);

        [OperationContract]
        List<CStock> ReadStockOfProduct(string productCode,string unitCode, string unit,decimal unitValue,string billNo,string fCode);

    }


    
    [DataContract]
    public class CStockDeletion
    {
        int id;
        string billNo;
        DateTime billDateTime = new DateTime();
        string narration;
        string financialCode;
        List<CStockDeletionDetails> details= new List<CStockDeletionDetails>();
 
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
        public string Narration
        {
            get { return narration; }
            set { narration = value; }
        }
        
        [DataMember]
        public string FinancialCode
        {
            get { return financialCode; }
            set { financialCode = value; }
        }

        [DataMember]
        public List<CStockDeletionDetails> Details
        {
            get { return details; }
            set { details = value; }
        }
    }


    [DataContract]
    public class CStockDeletionDetails
    {
        int serialNo;
        string productCode;
        string product;
        string stockDeletionUnit;
        string stockDeletionUnitCode;
        decimal stockDeletionUnitValue;
        decimal quantity;
        decimal stockDeletionRate;
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
        public string StockDeletionUnit
        {
            get { return stockDeletionUnit; }
            set { stockDeletionUnit = value; }
        }

        [DataMember]
        public string StockDeletionUnitCode
        {
            get { return stockDeletionUnitCode; }
            set { stockDeletionUnitCode = value; }
        }

        [DataMember]
        public decimal StockDeletionUnitValue
        {
            get { return stockDeletionUnitValue; }
            set { stockDeletionUnitValue = value; }
        }

        [DataMember]
        public decimal Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        [DataMember]
        public decimal StockDeletionRate
        {
            get { return stockDeletionRate; }
            set { stockDeletionRate = value; }
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
