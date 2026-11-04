#import <mach/mach.h>
#import <dlfcn.h>

long long OS_PhysFootprintMB(void)
{
    task_vm_info_data_t info;
    mach_msg_type_number_t count = TASK_VM_INFO_COUNT;
    kern_return_t kr = task_info(mach_task_self(), TASK_VM_INFO, (task_info_t)&info, &count);
    if (kr != KERN_SUCCESS) return 0;
    return (long long)(info.phys_footprint / (1024ull * 1024ull));
}

long long OS_AvailableMemoryMB(void)
{
    typedef size_t (*AvailFn)(void);
    AvailFn fn = (AvailFn)dlsym(RTLD_DEFAULT, "os_proc_available_memory");
    if (fn == 0) return 0;
    return (long long)(fn() / (1024ull * 1024ull));
}
