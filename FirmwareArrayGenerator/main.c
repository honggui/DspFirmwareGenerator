#include <stdio.h>
#include <stdlib.h>
#include <string.h>
int main()
{
	FILE* fp;
	fp = fopen("./output/rkdsp.bin", "rb");
	if(fp){
		fseek(fp, 0, SEEK_END);
		int len = ftell(fp);
		fseek(fp, 0, SEEK_SET);
		unsigned char* buf = (unsigned char*) malloc(len);
		if (fread(buf, len, 1, fp) == 1){//ok
			FILE* fw;
			fw = fopen("./output/rkdsp_fw.h", "w");
			if (!fw){
				printf("error @ open file ./output/rkdsp_fw.h\n");
			}
			else{
				fprintf(fw, "static const uint8_t dsp_fw[] = {");
				for(int i = 0; i < len; i++){
					if(!(i&7)){
						fprintf(fw, "\n");
					}
					fprintf(fw, " 0x%02x,", buf[i]);
				}
				fprintf(fw, "\n};\n");
				fclose(fw);
			}
		}
		else{
			printf("error @ read file %s\n", "./output/rkdsp.bin");
		}
                fclose(fp);
	}
	return 0;
}
