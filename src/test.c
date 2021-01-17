
int main() {

    int a = 5;
    int* b;
    b = &a;
    int* c = b;
    int* d = &a;
    int** e = &b;
    int f = *b;
    int g = *c;

    **e = 2;

    printf(*(&(**e)));
    printf("Done");

    return 0;
}
