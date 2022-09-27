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
    public class PurchaseService : IPurchase
    {
        private string mBillType = "P";

        public bool CreateBill(CPurchase oPurchase)
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


                        int cbillNo = bs.ReadNextPurchaseBillNo(oPurchase.FinancialCode);
                        bs.UpdatePurchaseBillNo(oPurchase.FinancialCode,cbillNo+1);

                        List<string> barcodes = new BarcodeService().ReadBarcodes(oPurchase.Details.Count);
                        
                        for (int i = 0; i < oPurchase.Details.Count; i++)
                        {
                            product_transactions pt = new product_transactions();

                            pt.bill_no= cbillNo.ToString();
                            pt.bill_type = mBillType;
                            pt.bill_date_time = oPurchase.BillDateTime;
                            pt.supplier_code = oPurchase.SupplierCode;
                            pt.supplier = oPurchase.Supplier;
                            pt.supplier_address = oPurchase.SupplierAddress;
                            pt.narration = oPurchase.Narration;
                            pt.advance = oPurchase.Advance;
                            pt.extra_charges = oPurchase.Expense;
                            pt.discounts = oPurchase.Discount;
                            pt.financial_code = oPurchase.FinancialCode;

                            pt.serial_no = oPurchase.Details.ElementAt(i).SerialNo;
                            pt.product_code = oPurchase.Details.ElementAt(i).ProductCode;
                            pt.product = oPurchase.Details.ElementAt(i).Product;
                            pt.purchase_unit = oPurchase.Details.ElementAt(i).PurchaseUnit;
                            pt.purchase_unit_code = oPurchase.Details.ElementAt(i).PurchaseUnitCode;
                            pt.purchase_unit_value = oPurchase.Details.ElementAt(i).PurchaseUnitValue;
                            pt.quantity = oPurchase.Details.ElementAt(i).Quantity;
                            pt.purchase_rate = oPurchase.Details.ElementAt(i).PurchaseRate;
                            pt.mrp = oPurchase.Details.ElementAt(i).MRP;
                            //get a barcode here
                            pt.barcode = barcodes.ElementAt(i);                            
                            pt.unit_code= oPurchase.Details.ElementAt(i).PurchaseUnitCode;
                            pt.unit_value= oPurchase.Details.ElementAt(i).PurchaseUnitValue;

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
                        //get barcodes already used in other transactions
                        BarcodeService bs = new BarcodeService();
                        List<string> usedBarcodes = ReadAlreadyUsedBarcodes(billNo,financialCode);
                        //Remove editable entries of transaction
                        var cpp = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == billNo && x.financial_code == financialCode && x.bill_type == mBillType && !usedBarcodes.Contains<string>(x.barcode));
                        dataB.product_transactions.RemoveRange(cpp);

                        //Updating serial numbers of entires that are already used
                        int serialNo = 1;
                        var tr = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == billNo && x.financial_code == financialCode && x.bill_type == mBillType && usedBarcodes.Contains<string>(x.barcode));
                        foreach (var item in tr)
                        {
                            item.serial_no = serialNo++;
                        }

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

        public CPurchase ReadBill(string billNo,string financialCode)
        {
            CPurchase ccp = null;

            using (var dataB = new Database9001Entities())
            {
                var cps = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == billNo && x.financial_code == financialCode&&x.bill_type==mBillType).OrderBy(y=>y.serial_no);
                
                if (cps.Count() > 0)
                {
                    ccp = new CPurchase();

                    var cp = cps.FirstOrDefault();
                    ccp.Id = cp.id;
                    ccp.BillNo = cp.bill_no;
                    ccp.BillDateTime = cp.bill_date_time;
                    ccp.SupplierCode = cp.supplier_code;
                    ccp.Supplier = cp.supplier;
                    ccp.SupplierAddress = cp.supplier_address;
                    ccp.Narration = cp.narration;
                    ccp.Advance = (decimal)cp.advance;
                    ccp.Expense = (decimal)cp.extra_charges;
                    ccp.Discount = (decimal)cp.discounts;
                    ccp.FinancialCode = cp.financial_code;

                    foreach (var item in cps)
                    {
                        ccp.Details.Add(new CPurchaseDetails() { SerialNo=(int)item.serial_no,ProductCode=item.product_code,Product=item.product, PurchaseUnit=item.purchase_unit, PurchaseUnitCode=item.purchase_unit_code, PurchaseUnitValue = (decimal)item.purchase_unit_value, Quantity=(decimal)item.quantity, PurchaseRate = (decimal)item.purchase_rate, MRP = (decimal)item.mrp, Total=(decimal)(item.quantity*item.purchase_rate), Barcode = item.barcode});
                    }
                }
                
            }

            return ccp;
        }

        public int ReadNextBillNo(string financialCode)
        {
            
            BillNoService bns = new BillNoService();
            return bns.ReadNextPurchaseBillNo(financialCode);
            
        }

        public bool UpdateBill(CPurchase oPurchase)
        {
            bool returnValue = false;

            lock (Synchronizer.@lock)
            {
                using (var dataB = new Database9001Entities())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {
                        //get barcodes already used in other transactions
                        BarcodeService bs = new BarcodeService();                        
                        List<string> usedBarcodes =ReadAlreadyUsedBarcodes(oPurchase.BillNo,oPurchase.FinancialCode);
                        //Remove editable entries of transaction
                        var cpp = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == oPurchase.BillNo&& x.financial_code==oPurchase.FinancialCode&&x.bill_type==mBillType && !usedBarcodes.Contains<string>(x.barcode));
                        dataB.product_transactions.RemoveRange(cpp);
                        
                        int serialNo = 1;
                        for (int i = 0; i < oPurchase.Details.Count; i++)
                        {
                            if (!usedBarcodes.Contains<string>(oPurchase.Details.ElementAt(i).Barcode))
                            {
                                product_transactions pt = new product_transactions();

                                pt.bill_no = oPurchase.BillNo;
                                pt.bill_type = mBillType;
                                pt.bill_date_time = oPurchase.BillDateTime;
                                pt.supplier_code = oPurchase.SupplierCode;
                                pt.supplier = oPurchase.Supplier;
                                pt.supplier_address = oPurchase.SupplierAddress;
                                pt.narration = oPurchase.Narration;
                                pt.advance = oPurchase.Advance;
                                pt.extra_charges = oPurchase.Expense;
                                pt.discounts = oPurchase.Discount;
                                pt.financial_code = oPurchase.FinancialCode;

                                pt.serial_no = serialNo++;
                                pt.product_code = oPurchase.Details.ElementAt(i).ProductCode;
                                pt.product = oPurchase.Details.ElementAt(i).Product;
                                pt.purchase_unit = oPurchase.Details.ElementAt(i).PurchaseUnit;
                                pt.purchase_unit_code = oPurchase.Details.ElementAt(i).PurchaseUnitCode;
                                pt.purchase_unit_value = oPurchase.Details.ElementAt(i).PurchaseUnitValue;
                                pt.quantity = oPurchase.Details.ElementAt(i).Quantity;
                                pt.purchase_rate = oPurchase.Details.ElementAt(i).PurchaseRate;
                                pt.mrp = oPurchase.Details.ElementAt(i).MRP;
                                //get a barcode here
                                //MessageBox.Show(oPurchase.Details.ElementAt(i).Barcode);
                                pt.barcode = oPurchase.Details.ElementAt(i).Barcode != "" ?  oPurchase.Details.ElementAt(i).Barcode: bs.ReadNextBarcode();
                                pt.unit_code = oPurchase.Details.ElementAt(i).PurchaseUnitCode;
                                pt.unit_value = oPurchase.Details.ElementAt(i).PurchaseUnitValue;

                                dataB.product_transactions.Add(pt);
                            }
                        }

                        //Updating serial numbers of entires that are already used
                        var tr = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == oPurchase.BillNo && x.financial_code == oPurchase.FinancialCode && x.bill_type == mBillType && usedBarcodes.Contains<string>(x.barcode));
                        foreach (var item in tr)
                        {
                            item.serial_no = serialNo++;
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

        private List<string> ReadAlreadyUsedBarcodes(string billNo,string fCode)
        {
            List<string> barcodes = new List<string>();
            List<string> usedBarcodes = new List<string>();
            using (var dataB = new Database9001Entities())
            {
                try
                {
                    var bar = dataB.product_transactions.Select(c => new { c.barcode, c.bill_type, c.bill_no, c.financial_code }).Where(x => x.bill_type == mBillType && x.bill_no == billNo && x.financial_code == fCode);
                    barcodes = bar.Select(e => e.barcode).ToList<string>();

                    var usedBar = dataB.product_transactions.Select(c => new { c.barcode, c.bill_type }).Where(x => x.bill_type != mBillType && barcodes.Contains<string>(x.barcode));
                    usedBarcodes = usedBar.Select(e => e.barcode).ToList<string>();
                }
                catch
                {

                }
            }
            return usedBarcodes;
        }
    }
}
