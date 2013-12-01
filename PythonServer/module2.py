###########################################################################
# program: srs_gui.py
# author: Tom Irvine
# Email: tom@vibrationdata.com
# version: 1.4
# date: September 11, 2013
# description:  shock response spectrum for base excitation
#
###########################################################################
# 
# Note:  for use within Spyder IDE, set: 
#    
# Run > Configuration > Interpreter >
#    
# Excecute in an external system terminal
#
################################################################################

from __future__ import print_function
    
import sys

if sys.version_info[0] == 2:
    print ("Python 2.x")
    import Tkinter as tk
    from tkFileDialog import asksaveasfilename,askopenfilename

           
if sys.version_info[0] == 3:
    print ("Python 3.x")    
    import tkinter as tk 
    from tkinter.filedialog import asksaveasfilename,askopenfilename 

import os
import re
import numpy as np


import matplotlib.pyplot as plt

from scipy.signal import lfilter

from sys import stdin

################################################################################

class tk_SRS:
    def __init__(self,parent): 
        self.master=parent        # store the parent
        top = tk.Frame(parent)    # frame for all class widgets
        top.pack(side='top')      # pack frame in parent's window
        
        self.num=0
        self.num_fn=0
        
        self.a=[]
        self.b=[]
        self.dt=0
        self.sr=0  
        self.a_pos=[]
        self.a_neg=[]
        self.a_abs=[]
        self.pv_pos=[]
        self.pv_neg=[]
        self.pv_abs=[]    
        self.rd_pos=[]
        self.rd_neg=[]
        self.rd_abs=[] 
        self.fn=[] 
        self.omega=[]
        self.damp=0        
        
        self.hwtext1=tk.Label(top,text='Shock Response Spectrum for Base Excitation')
        self.hwtext1.grid(row=0, column=0, columnspan=6, pady=10,sticky=tk.W)

        self.hwtext2=tk.Label(top,text='The input file must have two columns:  time(sec) & accel(G)')
        self.hwtext2.grid(row=1, column=0, columnspan=6, pady=10,sticky=tk.W)

###############################################################################

        self.button_read = tk.Button(top, text="Read Input File", command=self.read_data)
        self.button_read.config( height = 2, width = 15 )
        self.button_read.grid(row=2, column=0,columnspan=1, pady=20,sticky=tk.W)  

        self.hwtextQ=tk.Label(top,text='Q=')
        self.hwtextQ.grid(row=2, column=1,padx=1,sticky=tk.E)

        self.Qr=tk.StringVar()  
        self.Qr.set('10')  
        self.Q_entry=tk.Entry(top, width = 5,textvariable=self.Qr)
        self.Q_entry.grid(row=2, column=2,sticky=tk.W)

###############################################################################

        self.hwtextLbx=tk.Label(top,text='Select Units')
        self.hwtextLbx.grid(row=3, column=0,padx=3)

        self.hwtextLbx=tk.Label(top,text='SRS Plot Type')
        self.hwtextLbx.grid(row=3, column=2,padx=5)

###############################################################################

        crow=4

        self.Lb1 = tk.Listbox(top,height=2,exportselection=0)
        self.Lb1.insert(1, "G, in/sec, in")
        self.Lb1.insert(2, "G, m/sec, mm")
        self.Lb1.grid(row=crow, column=0, pady=1)
        self.Lb1.select_set(0) 

        self.Lb2 = tk.Listbox(top,height=2,exportselection=0)
        self.Lb2.insert(1, "Pos & Neg")
        self.Lb2.insert(2, "Absolute")
        self.Lb2.grid(row=crow, column=2,padx=5, pady=1)
        self.Lb2.select_set(0) 

###############################################################################

        crow=5

        self.hwtextLb2_mr=tk.Label(top,text='Min Freq (Hz)')
        self.hwtextLb2_mr.grid(row=crow, column=0,padx=5, pady=8)

        self.hwtextf2=tk.Label(top,text='Max Freq (Hz)')
        self.hwtextf2.grid(row=crow, column=2,padx=5, pady=8)

###############################################################################

        crow=6

        self.f1r=tk.StringVar()  
        self.f1r.set('')  
        self.f1_entry=tk.Entry(top, width = 8,textvariable=self.f1r)
        self.f1_entry.grid(row=crow, column=0,padx=5, pady=1)

        self.f2r=tk.StringVar()  
        self.f2r.set('')  
        self.f2_entry=tk.Entry(top, width = 8,textvariable=self.f2r)
        self.f2_entry.grid(row=crow, column=2,padx=5, pady=1)

###############################################################################

        crow=7

        self.button_calculate = tk.Button(top, text="Calculate", command=self.srs_calculation)
        self.button_calculate.config( height = 2, width = 15,state = 'disabled')
        self.button_calculate.grid(row=crow, column=0,columnspan=2, pady=20) 

        self.button_quit=tk.Button(top, text="Quit", command=lambda root=root:quit(root))
        self.button_quit.config( height = 2, width = 15 )
        self.button_quit.grid(row=crow, column=2,columnspan=2, padx=10,pady=20)

################################################################################

        crow=8

        self.hwtextext_exsrs=tk.Label(top,text='Export SRS Data')
        self.hwtextext_exsrs.grid(row=crow, column=0,pady=10)  
        self.hwtextext_exsrs.config(state = 'disabled')

################################################################################
    
        crow=9

        self.button_sa = tk.Button(top, text="Acceleration", command=self.export_accel)
        self.button_sa.config( height = 2, width = 15,state = 'disabled' )
        self.button_sa.grid(row=crow, column=0,columnspan=2, pady=3, padx=1)  

        self.button_spv = tk.Button(top, text="Pseudo Velocity", command=self.export_pv)
        self.button_spv.config( height = 2, width = 15,state = 'disabled' )
        self.button_spv.grid(row=crow, column=2,columnspan=2, pady=3, padx=1) 

        self.button_srd = tk.Button(top, text="Rel Disp", command=self.export_rd)
        self.button_srd.config( height = 2, width = 15,state = 'disabled' )
        self.button_srd.grid(row=crow, column=4,columnspan=2, pady=3, padx=1) 
            
################################################################################            

    def read_data(self):            
            
        self.a,self.b,self.num=read_two_columns_from_dialog('Select Acceleration File')
        
        dur=self.a[self.num-1]-self.a[0]
        self.dt=dur/float(self.num)
        
        self.sr=1./self.dt
        
        self.sr,self.dt=sample_rate_check(self.a,self.b,self.num,self.sr,self.dt)
        
        plt.ion()
        plt.clf()
        plt.figure(1)

        plt.plot(self.a, self.b, linewidth=1.0,color='b')        # disregard error
       
        plt.grid(True)
        plt.xlabel('Time (sec)')
        plt.ylabel('Accel (G)')  
        plt.title('Base Input Time History')
    
        plt.draw()

        print ("\n samples = %d " % self.num)
        
        self.button_calculate.config(state = 'normal')    

    def srs_calculation(self):  

        Q=float(self.Qr.get())
        self.damp=1./(2.*Q);

        f1=float(self.f1r.get())
        f2=float(self.f2r.get())
           
        oct=1./12.
        
        fmax=min([f2,self.sr/8.])
        
        noct=np.log(fmax/f1)/np.log(2)
        
        self.num_fn=int(np.ceil(noct*12))
        
        self.fn=np.zeros(self.num_fn,'f')
         
        self.fn[0]=f1    
        for j in range(1,self.num_fn):
            self.fn[j]=self.fn[j-1]*(2.**oct)
                    
        self.omega=2*np.pi*self.fn
    

    
        self.a_pos,self.a_neg,self.a_abs= \
               tk_SRS.accel_SRS(self.b,self.num_fn,self.omega,self.damp,self.dt) 
               
        nLb1= int(self.Lb1.curselection()[0])              
        
        self.rd_pos,self.rd_neg,self.rd_abs,self.pv_pos,self.pv_neg,self.pv_abs=\
               self.rd_SRS(self.b,self.num_fn,self.omega,self.damp,self.dt,nLb1)        
        
           
        n= int(self.Lb2.curselection()[0]) 

        plt.ion()
        plt.close(2)
        plt.figure(2)
        
        if(n==0):    
            plt.plot(self.fn, self.a_pos, label="positive")
            plt.plot(self.fn, self.a_neg, label="negative")
            plt.legend(loc="upper left")      
        else:
            plt.plot(self.fn, self.a_abs, linewidth=1.0,color='b')        
   
        astr='Acceleration'
    
        title_string= astr + ' Shock Response Spectrum Q='+str(Q)     
   
        for i in range(1,200):
            if(Q==float(i)):
                title_string= astr +' Shock Response Spectrum Q='+str(i)
                break
       
        plt.grid(True)
        plt.xlabel('Natural Frequency (Hz)')
        plt.ylabel('Peak Accel (G)')   
        
        plt.title(title_string)
        plt.xscale('log')
        plt.yscale('log')
        plt.xlim([f1,f2])
            
        plt.draw()
    
        m= int(self.Lb1.curselection()[0])
    
        plt.ion()
        plt.close(3)        
        plt.figure(3)

        if(n==0):    
            plt.plot(self.fn, self.pv_pos, label="positive")
            plt.plot(self.fn, self.pv_neg, label="negative")
            plt.legend(loc="upper right")      
        else:
            plt.plot(self.fn, self.pv_abs, linewidth=1.0,color='b')        # disregard error
   
        astr='Pseudo Velocity'
    
        title_string= astr + ' Shock Response Spectrum Q='+str(Q)     
   
        for i in range(1,200):
            if(Q==float(i)):
                title_string= astr +' Shock Response Spectrum Q='+str(i)
                break
       
        plt.grid(True)
        plt.xlabel('Natural Frequency (Hz)')
    
        if(m==0):
            plt.ylabel('Peak Vel (in/sec)') 
        else:
            plt.ylabel('Peak Vel (m/sec)') 
        
        plt.title(title_string)
        plt.xscale('log')
        plt.yscale('log')
        plt.xlim([f1,f2])
            
        plt.draw()    
    
        plt.ion()
        plt.close(4)
        plt.figure(4)

        if(n==0):    
            plt.plot(self.fn, self.rd_pos, label="positive")
            plt.plot(self.fn, self.rd_neg, label="negative")
            plt.legend(loc="upper right")      
        else:
            plt.plot(self.fn, self.rd_abs, linewidth=1.0,color='b')   
   
        astr='Relative Displacement'
    
        title_string= astr + ' Shock Response Spectrum Q='+str(Q)     
   
        for i in range(1,200):
            if(Q==float(i)):
                title_string= astr +' Shock Response Spectrum Q='+str(i)
                break
       
        plt.grid(True)
        plt.xlabel('Natural Frequency (Hz)')
    
        if(m==0):
            plt.ylabel('Peak Rel Disp (in)') 
        else:
            plt.ylabel('Peak Rel Disp (mm)') 
        
        plt.title(title_string)
        plt.xscale('log')
        plt.yscale('log')
        plt.xlim([f1,f2])
            
        plt.draw()    

        self.hwtextext_exsrs.config(state = 'normal')
        self.button_sa.config(state = 'normal')
        self.button_spv.config(state = 'normal')    
        self.button_srd.config(state = 'normal')

################################################################################

    @classmethod    
    def accel_SRS(cls,b,num_fn,omega,damp,dt):
        
        a_pos=np.zeros(num_fn,'f')
        a_neg=np.zeros(num_fn,'f')
        a_abs=np.zeros(num_fn,'f')
    
        ac=np.zeros(3)     
        bc=np.zeros(3)

              
        for j in range(0,num_fn):
            
            omegad=omega[j]*np.sqrt(1.-(damp**2))
#
#  bc coefficients are applied to the excitation
            
            E=np.exp(-damp*omega[j]*dt)
            K=omegad*dt
            C=E*np.cos(K)
            S=E*np.sin(K)
            Sp=S/K

   
            ac[0]=1.   
            ac[1]=-2.*C
            ac[2]=+E**2   
        
            bc[0]=1.-Sp
            bc[1]=2.*(Sp-C)
            bc[2]=E**2-Sp
                             
            resp=lfilter(bc, ac, b, axis=-1, zi=None)            
#
            a_pos[j]= max(resp)
            a_neg[j]= abs(min(resp)) 
            a_abs[j]=max(abs(resp))  
            
        return a_pos,a_neg,a_abs    

#        print (" %8.4g  %8.4g " %(fn[j],a_abs[j]))          

    @classmethod  
    def rd_SRS(cls,b,num_fn,omega,damp,dt,n):
        
        rd_pos=np.zeros(num_fn,'f')
        rd_neg=np.zeros(num_fn,'f')
        rd_abs=np.zeros(num_fn,'f')
    
        ac=np.zeros(3)     
        bc=np.zeros(3)   

        for j in range(0,num_fn):
            
            omegad=omega[j]*np.sqrt(1.-(damp**2))            
            
            E =np.exp(  -damp*omega[j]*dt)
            E2=np.exp(-2*damp*omega[j]*dt)
             
            K=omegad*dt
            C=E*np.cos(K)
            S=E*np.sin(K)
        
            ac[0]=1   
            ac[1]=-2*C
            ac[2]=+E**2         
            
            Omr=(omega[j]/omegad)
            Omt=omega[j]*dt
            
            P=2*damp**2-1
            
            b00=2*damp*(C-1)
            b01=S*Omr*P
            b02=Omt
            
            b10=-2*Omt*C
            b11= 2*damp*(1-E2)
            b12=-2*b01   

            b20=(2*damp+Omt)*E2
            b21= b01
            b22=-2*damp*C               
            
            bc[0]=b00+b01+b02
            bc[1]=b10+b11+b12
            bc[2]=b20+b21+b22
            
            bc=-bc/(omega[j]**3*dt)
 
# ac same as acceleration case                       
            
            resp=lfilter(bc, ac, b, axis=-1, zi=None) 
        
            rd_pos[j]= max(resp)
            rd_neg[j]= abs(min(resp))   
            rd_abs[j]=max(abs(resp))

        pv_pos=omega*rd_pos
        pv_neg=omega*rd_neg
        pv_abs=omega*rd_abs

        if(n==0):
            pv_scale=386.
            rd_scale=386.
        else:
            pv_scale=9.81
            rd_scale=9.81*1000
                
        rd_pos*=rd_scale
        rd_neg*=rd_scale
        rd_abs*=rd_scale
    
        pv_pos*=pv_scale
        pv_neg*=pv_scale
        pv_abs*=pv_scale    
    
        return rd_pos,rd_neg,rd_abs,pv_pos,pv_neg,pv_abs 
   
################################################################################
    
    def export_accel(self):
        output_file_path = asksaveasfilename(parent=root,title="Enter the acceleration SRS filename")           
        output_file = output_file_path.rstrip('\n')    
        n= int(self.Lb2.curselection()[0])   
 
        if(n==0):
            WriteData3(self.num_fn,self.fn,self.a_pos,self.a_neg,output_file)        
        else:
            WriteData2(self.num_fn,self.fn,self.a_abs,output_file)

    def export_pv(self):
        output_file_path = asksaveasfilename(parent=root,title="Enter the pseudo velocity SRS filename")           
        output_file = output_file_path.rstrip('\n')    
        n= int(self.Lb2.curselection()[0])   
        if(n==0):
            WriteData3(self.num_fn,self.fn,self.pv_pos,self.pv_neg,output_file)        
        else:
            WriteData2(self.num_fn,self.fn,self.pv_abs,output_file)

    def export_rd(self):    
        output_file_path = asksaveasfilename(parent=root,title="Enter the relative displacement SRS filename")           
        output_file = output_file_path.rstrip('\n')    
        n= int(self.Lb2.curselection()[0])   
        if(n==0):
            WriteData3(self.num_fn,self.fn,self.rd_pos,self.rd_neg,output_file)        
        else:
            WriteData2(self.num_fn,self.fn,self.rd_abs,output_file)
            
    

        
################################################################################
################################################################################

def quit(root):
    root.destroy()

def read_two_columns_from_dialog(label):
    """
    Read data from file using a dialog box
    """ 
    while(1):

        input_file_path = askopenfilename(parent=root,title=label)

        file_path = input_file_path.rstrip('\n')
#
        if not os.path.exists(file_path):
            print ("This file doesn't exist")
#
        if os.path.exists(file_path):
            print ("This file exists")
            print (" ")
            infile = open(file_path,"rb")
            lines = infile.readlines()
            infile.close()

            a = []
            b = []
            num=0
            for line in lines:
#
                if sys.version_info[0] == 3:            
                    line = line.decode(encoding='UTF-8')                 
            
                if re.search(r"(\d+)", line):  # matches a digit
                    iflag=0
                else:
                    iflag=1 # did not find digit
#
                if re.search(r"#", line):
                    iflag=1
#
                if iflag==0:
                    line=line.lower()
                    if re.search(r"([a-d])([f-z])", line):  # ignore header lines
                        iflag=1
                    else:
                        line = line.replace(","," ")
                        col1,col2=line.split()
                        a.append(float(col1))
                        b.append(float(col2))
                        num=num+1
            break

            a=np.array(a)
            b=np.array(b)

            print ("\n samples = %d " % num)
            
    return a,b,num

def sample_rate_check(a,b,num,sr,dt):
    dtmin=1e+50
    dtmax=0

    for i in range(1, num-1):
        if (a[i]-a[i-1])<dtmin:
            dtmin=a[i]-a[i-1];
            if (a[i]-a[i-1])>dtmax:
                dtmax=a[i]-a[i-1];

    print ("  dtmin = %8.4g sec" % dtmin)
    print ("     dt = %8.4g sec" % dt)
    print ("  dtmax = %8.4g sec \n" % dtmax)

    srmax=float(1/dtmin)
    srmin=float(1/dtmax)

    print ("  srmax = %8.4g samples/sec" % srmax)
    print ("     sr = %8.4g samples/sec" % sr)
    print ("  srmin = %8.4g samples/sec" % srmin)

    if((srmax-srmin) > 0.01*sr):
        print(" ")
        print(" Warning: sample rate difference ")
        sr = None
        while not sr:
            try:
                print(" Enter new sample rate ")
                s = stdin.readline()
                sr=float(s)
                dt=1/sr
            except ValueError:
                print ('Invalid Number')
    return sr,dt

########################################################################

def WriteData2(nn,aa,bb,output_file_path):
    """
    Write two columns of data to an external ASCII text file
    """
    output_file = output_file_path.rstrip('\n')
    outfile = open(output_file,"w")
    for i in range (0, nn):
        outfile.write(' %10.6e \t %8.4e \n' %  (aa[i],bb[i]))
    outfile.close()

########################################################################


def WriteData3(nn,aa,bb,cc,output_file_path):
    """
    Write three columns of data to an external ASCII text file
    """
    outfile = open(output_file_path,"w")
    for i in range (0, nn):
        outfile.write(' %8.4e \t %8.4e \t %8.4e \n' %  (aa[i],bb[i],cc[i]))
    outfile.close()

################################################################################
           
root = tk.Tk()
root.minsize(400,400)
root.geometry("600x470")

root.title("srs_gui.py ver 1.4  by Tom Irvine") 

tk_SRS(root)
root.mainloop()    