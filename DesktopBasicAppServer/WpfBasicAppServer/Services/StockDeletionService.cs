using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows;
using WpfAccountServerApp.General;

namespace WpfAccountServerApp.Services
{   
    public class StockDeletionService : IStockDeletion
    {
        private string mBillType = "SD";

        public bool CreateBill(CStockDeletion oStockDeletion)
        {
            bool returnValue = false;

            lock (Synchronizer.@lock)
            {
                
                using (var dataB = new Database9001Entities())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {

                        ProductService ls = new ProductService();
                        BillNoService bs = new BillNoService();


                        int cbillNo = bs.ReadNextStockDeletionBillNo(oStockDeletion.FinancialCode);
                        bs.UpdateStockDeletionBillNo(oStockDeletion.FinancialCode,cbillNo+1);
                        
                        for (int i = 0; i < oStockDeletion.Details.Count; i++)
                        {
                            product_transactions pt = new product_transactions();

                            pt.bill_no= cbillNo.ToString();
                            pt.bill_type = mBillType;
                            pt.bill_date_time = oStockDeletion.BillDateTime;
                            pt.narration = oStockDeletion.Narration;
                            pt.financial_code = oStockDeletion.FinancialCode;

                            pt.serial_no = oStockDeletion.Details.ElementAt(i).SerialNo;
                            pt.product_code = oStockDeletion.Details.ElementAt(i).ProductCode;
                            pt.product = oStockDeletion.Details.ElementAt(i).Product;
                            pt.sales_unit = oStockDeletion.Details.ElementAt(i).StockDeletionUnit;
                            pt.sales_unit_code = oStockDeletion.Details.ElementAt(i).StockDeletionUnitCode;
                            pt.sales_unit_value = oStockDeletion.Details.ElementAt(i).StockDeletionUnitValue;
                            pt.quantity = oStockDeletion.Details.ElementAt(i).Quantity*-1;
                            pt.sales_rate = oStockDeletion.Details.ElementAt(i).StockDeletionRate;
                            pt.mrp = oStockDeletion.Details.ElementAt(i).MRP;
                            //get a barcode here
                            pt.barcode = oStockDeletion.Details.ElementAt(i).Barcode;
                            pt.unit_code= oStockDeletion.Details.ElementAt(i).StockDeletionUnitCode;
                            pt.unit_value= oStockDeletion.Details.ElementAt(i).StockDeletionUnitValue;

                            dataB.product_transactions.Add(pt);                            
                        }

                        dataB.SaveChanges();
                        //Success
                        returnValue = true;

                        dataBTransaction.Commit();
                    }
                    catch(Exception e)
                    {                        
                        dataBTransaction.Rollback();
                    }
                    finally
                    {

                    }
                }                
            }

            return returnValue;
        }

        public bool DeleteBill(string billNo,string financialCode)
        {
            bool returnValue = true;

            lock (Synchronizer.@lock)
            {
                using (var dataB = new Database9001Entities())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {
                        //Delete the transaction
                        var cpp = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == billNo && x.financial_code == financialCode && x.bill_type == mBillType);
                        dataB.product_transactions.RemoveRange(cpp);
                        
                        dataB.SaveChanges();                        
                        dataBTransaction.Commit();
                    }
                    catch
                    {
                        returnValue = false;
                        dataBTransaction.Rollback();
                    }
                    finally
                    {

                    }
                }
            }
            return returnValue;
        }

        public CStockDeletion ReadBill(string billNo,string financialCode)
        {
            CStockDeletion ccp = null;

            using (var dataB = new Database9001Entities())
            {
                var cps = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == billNo && x.financial_code == financialCode&&x.bill_type==mBillType).OrderBy(y=>y.serial_no);
                
                if (cps.Count() > 0)
                {
                    ccp = new CStockDeletion();

                    var cp = cps.FirstOrDefault();
                    ccp.Id = cp.id;
                    ccp.BillNo = cp.bill_no;
                    ccp.BillDateTime = cp.bill_date_time;
                    ccp.Narration = cp.narration;
                    ccp.FinancialCode = cp.financial_code;

                    foreach (var item in cps)
                    {
                        ccp.Details.Add(new CStockDeletionDetails() { SerialNo=(int)item.serial_no,ProductCode=item.product_code,Product=item.product, StockDeletionUnit=item.sales_unit, StockDeletionUnitCode=item.sales_unit_code, StockDeletionUnitValue = (decimal)item.sales_unit_value, Quantity=(decimal)item.quantity*-1, StockDeletionRate = (decimal)item.sales_rate, MRP = (decimal)item.mrp, Total=(decimal)(item.quantity*item.sales_rate*-1), Barcode = item.barcode});
                    }
                }
                
            }

            return ccp;
        }

        public int ReadNextBillNo(string financialCode)
        {
            
            BillNoService bns = new BillNoService();
            return bns.ReadNextStockDeletionBillNo(financialCode);
            
        }
        
        public bool UpdateBill(CStockDeletion oStockDeletion)
        {
            bool returnValue = false;

            lock (Synchronizer.@lock)
            {
                using (var dataB = new Database9001Entities())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {
                        var cpp = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == oStockDeletion.BillNo&& x.financial_code==oStockDeletion.FinancialCode&&x.bill_type==mBillType);
                        dataB.product_transactions.RemoveRange(cpp);

                        for (int i = 0; i < oStockDeletion.Details.Count; i++)
                        {

                            product_transactions pt = new product_transactions();

                            pt.bill_no = oStockDeletion.BillNo;
                            pt.bill_type = mBillType;
                            pt.bill_date_time = oStockDeletion.BillDateTime;
                            pt.narration = oStockDeletion.Narration;
                            pt.financial_code = oStockDeletion.FinancialCode;

                            pt.serial_no = oStockDeletion.Details.ElementAt(i).SerialNo;
                            pt.product_code = oStockDeletion.Details.ElementAt(i).ProductCode;
                            pt.product = oStockDeletion.Details.ElementAt(i).Product;
                            pt.sales_unit = oStockDeletion.Details.ElementAt(i).StockDeletionUnit;
                            pt.sales_unit_code = oStockDeletion.Details.ElementAt(i).StockDeletionUnitCode;
                            pt.sales_unit_value = oStockDeletion.Details.ElementAt(i).StockDeletionUnitValue;
                            pt.quantity = oStockDeletion.Details.ElementAt(i).Quantity*-1;
                            pt.sales_rate = oStockDeletion.Details.ElementAt(i).StockDeletionRate;
                            pt.mrp = oStockDeletion.Details.ElementAt(i).MRP;
                            //get a barcode here
                            pt.barcode = oStockDeletion.Details.ElementAt(i).Barcode;
                            pt.unit_code = oStockDeletion.Details.ElementAt(i).StockDeletionUnitCode;
                            pt.unit_value = oStockDeletion.Details.ElementAt(i).StockDeletionUnitValue;

                            dataB.product_transactions.Add(pt);

                        }

                        
                        dataB.SaveChanges();

                        //Success
                        returnValue = true;

                        dataBTransaction.Commit();
                    }
                    catch(Exception e)
                    {                        
                        dataBTransaction.Rollback();
                    }
                    finally
                    {

                    }
                }
            }
            return returnValue;
        }

        public List<CStock> ReadStockOfProduct(string productCode,string unitCode,string unit, decimal unitValue,string billNo,string fCode)
        {
            List<CStock> stocks = new List<CStock>();
            
            try
            {

                UnitService us = new UnitService();
                decimal lowestUnitVal = us.ReadLowestUnitValue(unitCode);

                using (var dataB = new Database9001Entities())
                {
                    var cps = dataB.product_transactions.Where(c => c.product_code==productCode && !(c.bill_no==billNo&&c.bill_type==mBillType&&c.financial_code==fCode)).GroupBy(x => new {x.barcode}, x => new {x.unit_value, x.quantity,x.barcode,x.mrp })
                        .Select(y => new {Quantity = y.Sum(x => x.quantity * (unitValue/x.unit_value)),Barcode = y.FirstOrDefault().barcode, MRP = y.Sum(x=>x.mrp/(unitValue/x.unit_value))/y.Count()});
                    foreach (var item in cps)
                    {                        
                        CStock cs = new CStock() { Barcode=item.Barcode, Quantity=(decimal)item.Quantity, Unit=unit, MRP=(decimal)item.MRP };
                        stocks.Add(cs);
                    }

                }                

            }
            catch (Exception e)
            {

            }
            
            return stocks;
        }
    }
}
